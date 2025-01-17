using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockW : MonoBehaviour
{
    public bool popo;
    public GameObject S1;
    public GameObject S2;

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
                //TODO:选中特效
            }
        }
        else if (S2 == null&& S1 != S2)
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                S2 = hit.collider.transform.gameObject;
                //TODO:选中特效
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

            Vector3 target = new Vector3();
            float i = S2.transform.position.y - S1.transform.position.y;
            target = S2.transform.position + new Vector3(i, -i, -i);
            foreach (var v in list)
            {
                if (Vector3.Distance(S1.transform.position + v, target) <= 0.1f)
                {
                    S2.transform.parent.position += new Vector3(i, -i, -i);
                }
            }
            S1 = null;
            S2 = null;

        }
        

    }



}
