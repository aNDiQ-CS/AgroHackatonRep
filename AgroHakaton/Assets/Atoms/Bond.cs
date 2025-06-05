using UnityEngine;

public class Bond : MonoBehaviour
{
    public Atom StartAtom { get; private set; }
    public Atom EndAtom { get; private set; }
    public int BondOrder { get; private set; } = 1;

    public bool IsAromatic { get {
            return BondOrder == 2 &&
               StartAtom != null &&
               EndAtom != null &&
               StartAtom.Symbol == "C" &&
               EndAtom.Symbol == "C";
        } set {
            IsAromatic = value;
        } 
    }
    private LineRenderer _lineRenderer;
    private bool _isInitialized;

    public void Initialize(Atom startAtom, Atom endAtom, int bondOrder = 1)
    {
        StartAtom = startAtom;
        EndAtom = endAtom;
        BondOrder = bondOrder;

        // Создаем и настраиваем LineRenderer
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.startWidth = 0.08f;
        _lineRenderer.endWidth = 0.08f;
        _lineRenderer.material = new Material(Shader.Find("Standard"));
        _lineRenderer.material.color = Color.black;
        _lineRenderer.positionCount = 2;
        _lineRenderer.useWorldSpace = true;

        // Добавляем коллайдер для взаимодействия
        AddCollider();

        UpdateVisual();
        _isInitialized = true;
    }

    private void AddCollider()
    {
        CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
        collider.radius = 0.15f;
        collider.direction = 2; // Z-axis
        collider.isTrigger = true;

        UpdateCollider();
    }

    public void UpdateVisual()
    {
        if (!_isInitialized || StartAtom == null || EndAtom == null) return;

        Vector3 startPos = StartAtom.transform.position;
        Vector3 endPos = EndAtom.transform.position;
        Vector3 direction = (endPos - startPos).normalized;

        // Удаляем старые линии
        ClearBondLines();

        // Создаем линии в зависимости от типа связи
        switch (BondOrder)
        {
            case 1:
                CreateSingleBond(startPos, endPos);
                break;
            case 2:
                CreateDoubleBond(startPos, endPos, direction);
                break;
            case 3:
                CreateTripleBond(startPos, endPos, direction);
                break;
        }

        // Обновляем коллайдер
        UpdateCollider();
    }

    private void ClearBondLines()
    {
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("BondLine"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void CreateSingleBond(Vector3 start, Vector3 end)
    {
        CreateBondLine("BondLine_Single", start, end, 0.1f, Color.black);
    }

    private void CreateDoubleBond(Vector3 start, Vector3 end, Vector3 direction)
    {
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;
        float offset = 0.07f;

        CreateBondLine("BondLine_Double1",
            start + perpendicular * offset,
            end + perpendicular * offset,
            0.07f, Color.black);

        CreateBondLine("BondLine_Double2",
            start - perpendicular * offset,
            end - perpendicular * offset,
            0.07f, Color.black);
    }

    private void CreateTripleBond(Vector3 start, Vector3 end, Vector3 direction)
    {
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;
        float offset = 0.1f;

        // Центральная линия
        CreateBondLine("BondLine_Triple1", start, end, 0.07f, Color.black);

        // Боковые линии
        CreateBondLine("BondLine_Triple2",
            start + perpendicular * offset,
            end + perpendicular * offset,
            0.07f, Color.black);

        CreateBondLine("BondLine_Triple3",
            start - perpendicular * offset,
            end - perpendicular * offset,
            0.07f, Color.black);
    }

    private void CreateBondLine(string name, Vector3 start, Vector3 end, float width, Color color)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(transform);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.startWidth = width;
        lr.endWidth = width;
        lr.material = new Material(Shader.Find("Standard"));
        lr.material.color = color;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    private void HandleBondTypes(Vector3 startPos, Vector3 endPos)
    {
        if (BondOrder == 1) return;

        // Для двойных/тройных связей создаем дополнительные линии
        Vector3 direction = (endPos - startPos).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;

        float offset = 0.1f;

        // Удаляем старые дочерние линии
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Создаем новые линии
        for (int i = 0; i < BondOrder - 1; i++)
        {
            GameObject lineObj = new GameObject($"BondLine_{i}");
            lineObj.transform.SetParent(transform);

            LineRenderer extraLine = lineObj.AddComponent<LineRenderer>();
            extraLine.startWidth = 0.08f;
            extraLine.endWidth = 0.08f;
            extraLine.material = new Material(Shader.Find("Standard"));
            extraLine.material.color = Color.black;
            extraLine.positionCount = 2;
            extraLine.useWorldSpace = true;

            // Смещение для двойных/тройных связей
            float currentOffset = offset * (i - (BondOrder - 2) / 2f);
            Vector3 offsetVec = perpendicular * currentOffset;

            extraLine.SetPosition(0, startPos + offsetVec);
            extraLine.SetPosition(1, endPos + offsetVec);
        }
    }

    private void UpdateCollider()
    {
        if (StartAtom == null || EndAtom == null) return;

        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        if (collider == null) return;

        Vector3 midPoint = (StartAtom.transform.position + EndAtom.transform.position) / 2;
        collider.center = transform.InverseTransformPoint(midPoint);

        float distance = Vector3.Distance(StartAtom.transform.position, EndAtom.transform.position);
        collider.height = distance + collider.radius * 2;

        transform.LookAt(StartAtom.transform.position);
    }

    public void SetBondOrder(int order)
    {
        BondOrder = Mathf.Clamp(order, 1, 3);
        UpdateVisual();
    }

    public void CycleBondOrder()
    {
        BondOrder = BondOrder % 3 + 1;
        UpdateVisual();
    }

    public void BreakBond()
    {
        if (StartAtom != null) StartAtom.RemoveBond(this);
        if (EndAtom != null) EndAtom.RemoveBond(this);
        Destroy(gameObject);
    }

    public void SetAromaticMaterial(Material material)
    {
        // Применяем материал ко всем линиям связи
        foreach (Transform child in transform)
        {
            LineRenderer lr = child.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.material = material;
            }
        }
    }
    /*public bool IsAromatic()
    {
        // Для упрощения считаем все связи в 6-членных циклах ароматическими
        return BondOrder == 2 &&
               StartAtom != null &&
               EndAtom != null &&
               StartAtom.Symbol == "C" &&
               EndAtom.Symbol == "C";
    }*/

    private void Update()
    {
        if (_isInitialized && (StartAtom == null || EndAtom == null))
        {
            BreakBond();
        }
        else if (_isInitialized)
        {
            UpdateVisual();
        }
    }
}