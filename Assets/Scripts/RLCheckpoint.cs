using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using UnityEngine;

public class RLCheckpoint : MonoBehaviour
{

    public RLCheckpoint next_checkpoint;

    private void OnTriggerEnter(Collider other)
    {
        if (next_checkpoint != null)
        {
            //Destroy(this.gameObject);
            other.GetComponent<VTOLFlightControlAgent>().Target = next_checkpoint.transform;
            other.GetComponent<VTOLFlightControlAgent>().timer = 0;
            
        }
        else
        {
            Debug.Log("Win");
        }
        
    }
}
