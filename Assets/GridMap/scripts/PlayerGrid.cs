using System.Collections.Generic;
using UnityEngine;


public class PlayerGrid : MonoBehaviour
{
    private float speed = 40f;
    private int currentPathIndex;
    private List<Vector3> pathVectorList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if(pathVectorList != null)
        {
            
            Vector3 targetPosition = pathVectorList[currentPathIndex];
            if(Vector3.Distance(transform.position, targetPosition) > 1f)
            {
              Vector3 moveDir = (targetPosition - transform.position).normalized;
              float distanceBefore = Vector3.Distance(transform.position, targetPosition);  
                transform.position = transform.position + speed * Time.deltaTime * moveDir;

            } else
            {
                currentPathIndex++;
                if(currentPathIndex >= pathVectorList.Count)
                {
                    StopMoving();
                }
            }
        }
    }

    private void StopMoving()
    {
        pathVectorList = null;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

     public void SetTargetPosition(Vector3 targetPosition)
    {
        pathVectorList = PathFinding.Instance.FindPath(GetPosition(), targetPosition);
        currentPathIndex = 0;
        if(pathVectorList != null && pathVectorList.Count > 1)
        {
            pathVectorList.RemoveAt(0);
        }
    }
}
