using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Classic
{
    public class NavAgentController : MonoBehaviour
{
    [SerializeField]
    private Camera Camera;

    [SerializeField] 
    private NavMeshAgent Agent;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
            if ( !Physics.Raycast(ray, out var result))
            {
                return;
            }

            Agent.SetDestination(result.point);
        }
    }
}
}
