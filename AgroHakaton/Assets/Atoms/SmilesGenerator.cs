using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SmilesGenerator
{
    private int _currentCycleIndex = 1;
    private Dictionary<Bond, int> _cycleBonds = new Dictionary<Bond, int>();
    private HashSet<Atom> _visited = new HashSet<Atom>();
    private Dictionary<Atom, List<Atom>> _graph = new Dictionary<Atom, List<Atom>>();
    private Dictionary<Atom, List<int>> _pendingRingClosures = new Dictionary<Atom, List<int>>();
    private Dictionary<Atom, int> _atomPosition = new Dictionary<Atom, int>();
    private List<string> _tokens = new List<string>();
    private HashSet<Atom> _mainChainSet = new HashSet<Atom>();
    private Dictionary<Atom, Atom> _mainChainNext = new Dictionary<Atom, Atom>();

    public string GenerateSmiles(List<Atom> atoms)
    {
        // Сброс состояния
        _currentCycleIndex = 1;
        _cycleBonds.Clear();
        _visited.Clear();
        _graph.Clear();
        _pendingRingClosures.Clear();
        _atomPosition.Clear();
        _tokens.Clear();
        _mainChainSet.Clear();
        _mainChainNext.Clear();

        // Построение графа (игнорируем водороды)
        foreach (Atom atom in atoms)
        {
            if (atom.Symbol == "H") continue;

            if (!_graph.ContainsKey(atom))
                _graph[atom] = new List<Atom>();

            foreach (Bond bond in atom.bonds)
            {
                Atom neighbor = bond.StartAtom == atom ? bond.EndAtom : bond.StartAtom;
                if (neighbor.Symbol == "H") continue;

                if (!_graph[atom].Contains(neighbor))
                    _graph[atom].Add(neighbor);
            }
        }

        // Поиск самой длинной цепи
        List<Atom> mainChain = FindLongestPath();
        if (mainChain.Count == 0) return "";
        _mainChainSet = new HashSet<Atom>(mainChain);

        // Установка следующих атомов в цепи
        for (int i = 0; i < mainChain.Count - 1; i++)
        {
            _mainChainNext[mainChain[i]] = mainChain[i + 1];
        }

        // Обработка основной цепи
        TraverseMainChain(mainChain);
        return string.Join("", _tokens);
    }

    private List<Atom> FindLongestPath()
    {
        List<Atom> longestPath = new List<Atom>();
        if (_graph.Count == 0) return longestPath;

        // Поиск начального атома (гетероатомы имеют приоритет)
        Atom startAtom = _graph.Keys
            .OrderBy(a => a.Symbol == "C" ? 1 : 0)
            .ThenByDescending(a => _graph[a].Count)
            .First();

        // DFS для поиска максимального пути
        foreach (var atom in _graph.Keys)
        {
            FindLongestPathDFS(atom, new HashSet<Atom>(), new List<Atom>(), ref longestPath);
        }
        return longestPath;
    }

    private void FindLongestPathDFS(Atom current, HashSet<Atom> visited, List<Atom> currentPath, ref List<Atom> longestPath)
    {
        visited.Add(current);
        currentPath.Add(current);

        if (currentPath.Count > longestPath.Count)
        {
            longestPath = new List<Atom>(currentPath);
        }

        foreach (var neighbor in _graph[current])
        {
            if (!visited.Contains(neighbor))
            {
                FindLongestPathDFS(neighbor, visited, currentPath, ref longestPath);
            }
        }

        visited.Remove(current);
        currentPath.RemoveAt(currentPath.Count - 1);
    }

    private void TraverseMainChain(List<Atom> mainChain)
    {
        for (int i = 0; i < mainChain.Count; i++)
        {
            Atom current = mainChain[i];
            _visited.Add(current);

            // Запись атома и его позиции
            _atomPosition[current] = _tokens.Count;
            WriteAtom(current);
            ProcessPendingRingClosures(current);

            // Получаем всех соседей (кроме следующего в цепи)
            List<Atom> neighbors = _graph[current]
                .Where(n => n != GetNextInChain(current))
                .ToList();

            // Обработка кольцевых связей
            foreach (Atom neighbor in neighbors.ToList())
            {
                if (_visited.Contains(neighbor))
                {
                    Bond bond = FindBond(current, neighbor);
                    if (bond == null || _cycleBonds.ContainsKey(bond)) continue;

                    int ringNum = GetNextRingNumber();
                    _cycleBonds[bond] = ringNum;
                    ScheduleRingClosure(current, ringNum);
                    ScheduleRingClosure(neighbor, ringNum);
                    neighbors.Remove(neighbor);
                }
            }

            // Обработка радикалов (непосещенные соседи)
            List<Atom> radicals = neighbors
                .Where(n => !_visited.Contains(n))
                .OrderBy(n => n.Symbol == "C" ? 1 : 0)
                .ThenByDescending(n => _graph[n].Count)
                .ToList();

            // Добавляем все радикалы сразу после атома
            foreach (Atom radical in radicals)
            {
                _tokens.Add("(");
                WriteBondType(FindBond(current, radical));
                ProcessBranch(radical, current);
                _tokens.Add(")");
            }

            // Добавляем связь к следующему атому цепи
            if (i < mainChain.Count - 1)
            {
                Atom next = mainChain[i + 1];
                WriteBondType(FindBond(current, next));
            }
        }
    }

    private void ProcessBranch(Atom current, Atom parent)
    {
        _visited.Add(current);
        _atomPosition[current] = _tokens.Count;
        WriteAtom(current);
        ProcessPendingRingClosures(current);

        // Обработка всех соседей (кроме родителя)
        List<Atom> neighbors = _graph[current]
            .Where(n => n != parent)
            .ToList();

        // Обработка кольцевых связей
        foreach (Atom neighbor in neighbors.ToList())
        {
            if (_visited.Contains(neighbor))
            {
                Bond bond = FindBond(current, neighbor);
                if (bond == null || _cycleBonds.ContainsKey(bond)) continue;

                int ringNum = GetNextRingNumber();
                _cycleBonds[bond] = ringNum;
                ScheduleRingClosure(current, ringNum);
                ScheduleRingClosure(neighbor, ringNum);
                neighbors.Remove(neighbor);
            }
        }

        // Рекурсивная обработка оставшихся соседей
        foreach (Atom neighbor in neighbors.Where(n => !_visited.Contains(n)))
        {
            WriteBondType(FindBond(current, neighbor));
            ProcessBranch(neighbor, current);
        }
    }

    private Atom GetNextInChain(Atom atom)
    {
        return _mainChainNext.TryGetValue(atom, out Atom next) ? next : null;
    }

    private void ScheduleRingClosure(Atom atom, int ringNumber)
    {
        if (!_pendingRingClosures.ContainsKey(atom))
            _pendingRingClosures[atom] = new List<int>();

        _pendingRingClosures[atom].Add(ringNumber);
    }

    private void ProcessPendingRingClosures(Atom atom)
    {
        if (_pendingRingClosures.TryGetValue(atom, out List<int> closures))
        {
            int position = _atomPosition[atom];
            closures.Sort();

            foreach (int ringNum in closures)
            {
                _tokens.Insert(position + 1, ringNum.ToString());
                position++;
            }

            _pendingRingClosures.Remove(atom);
        }
    }

    private int GetNextRingNumber()
    {
        return _currentCycleIndex++ % 100;
    }

    private Bond FindBond(Atom a1, Atom a2)
    {
        return a1.bonds.FirstOrDefault(b =>
            (b.StartAtom == a1 && b.EndAtom == a2) ||
            (b.StartAtom == a2 && b.EndAtom == a1));
    }

    private void WriteAtom(Atom atom)
    {
        string[] organicSubset = { "B", "C", "N", "O", "P", "S", "F", "Cl", "Br", "I" };
        _tokens.Add(organicSubset.Contains(atom.Symbol) ?
            atom.Symbol :
            $"[{atom.Symbol}]");
    }

    private void WriteBondType(Bond bond)
    {
        if (bond == null) return;

        switch (bond.BondOrder)
        {
            case 2: _tokens.Add("="); break;
            case 3: _tokens.Add("#"); break;
            case 4: _tokens.Add(":"); break; // Арооматические связи
        }
    }
}