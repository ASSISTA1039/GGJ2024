using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockW : MonoBehaviour
{
    public bool popo;
    public GameObject S1;
    public GameObject S2;
    public GameObject S1select;
    public GameObject S2select;

    private void Update()
    {
        if (popo&& Input.GetMouseButtonDown(1))
        {
            Select();
        }
    }

    public void Select()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (S1 == null && S2 ==null)
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                S1 = hit.collider.transform.gameObject;
                S1select = Instantiate(Resources.Load<GameObject>("Prefabs/Select/outline"));
                S1select.transform.position = S1.transform.position;
                //TODO:选中特效
            }
        }
        else if (S2 == null&& S1 != S2)
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                S2 = hit.collider.transform.gameObject;
                S2select = Instantiate(Resources.Load<GameObject>("Prefabs/Select/outline"));
                S2select.transform.position = S2.transform.position;
            }
        }
        else if(S1 != null && S2 != null)
        {
            //012,-112,-212,-211,-210
            List<Vector3> list = new List<Vector3>();
            list.Add(new Vector3(1,0,1));
            list.Add(new Vector3(0,0,1));
            list.Add(new Vector3(-1,0,1));
            list.Add(new Vector3(-1,0,0));
            list.Add(new Vector3(-1,0,-1));

            if (S2.transform.position.y - S1.transform.position.y > 0)
            {
                float i = S2.transform.position.y - S1.transform.position.y;
                Vector3 target = S2.transform.position + new Vector3(i, -i, -i);
                foreach (var v in list)
                {
                    if (Vector3.Distance(S1.transform.position + v, target) <= 0.1f)
                    {
                        if (S2.transform.parent = S1.transform.parent)
                        {
                            S2.transform.position += new Vector3(i, -i, -i);
                        }
                        else
                        {
                            S2.transform.parent.position += new Vector3(i, -i, -i);
                        }
                    }
                }
            }
            else
            {
                float i = S1.transform.position.y - S2.transform.position.y;
                Vector3 target = S1.transform.position + new Vector3(i, -i, -i);
                foreach (var v in list)
                {
                    if (Vector3.Distance(S1.transform.position + v, target) <= 0.1f)
                    {
                        if (S2.transform.parent = S1.transform.parent)
                        {
                            S1.transform.position += new Vector3(i, -i, -i);
                        }
                        else
                        {
                            S1.transform.parent.position += new Vector3(i, -i, -i);
                        }
                    }
                }
            }
            S1 = null;
            S2 = null;
            Destroy(S1select);
            Destroy(S2select);
            S1select = null;
            S2select = null;
        }
        

    }



}
