using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandFLoat : MonoBehaviour
{ 
     #region ItemVisuals
        public Vector3 PosOffset = new Vector3(0,0.5f,0);
        public float Period = 2.2f;
        public float Amplitude = 0.08f;
        public float Steps = 100f;
        float RotationRate = 0f;
        float PhaseOffset;
        float Angle;
        Quaternion original;
        public bool Rotates = false;
        #endregion
     void Start()
        {
            PhaseOffset = Random.value;
            Angle = Random.value * 360f;
            original = transform.rotation;
            RotationRate = Rotates ? Random.Range(0, 3) * 15f : 0f;
        }
    void Update()
        {

                float phase = PhaseOffset + Time.time / Period;
                phase = 2.0f * Mathf.PI * (int)(phase * Steps) / Steps;
                float offset = Amplitude * Mathf.Cos(phase);
                transform.position = offset * Vector3.up+ PosOffset;

                Angle += RotationRate * Time.deltaTime;
                if (RotationRate != 0f)
                    transform.rotation = Quaternion.AngleAxis(Angle, Vector3.up) * original;
        }
}
