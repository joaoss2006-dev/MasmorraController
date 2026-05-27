using UnityEngine;

public class ColizorController : MonoBehaviour
{
    private BoxCollider box;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        box = gameObject.GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    public void ChecaColisoes()
    {
        BoxCollider box = GetComponent<BoxCollider>();

        Collider[] colisoes = Physics.OverlapBox(
            box.bounds.center,
            box.bounds.extents,
            transform.rotation
        );

        foreach (Collider other in colisoes)
        {
            if (other.CompareTag("cubo"))
            {
                other.transform.SetParent(transform);
            }
        }
    }
}
