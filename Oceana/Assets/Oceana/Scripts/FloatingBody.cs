using UnityEngine;

namespace Oceana
{
    [RequireComponent(typeof(Rigidbody))]
    public class FloatingBody : MonoBehaviour
    {
        [SerializeField]
        private bool m_AlignWithNormal;

        private OceanPhysics m_Ocean;

        private Transform m_Transform;
        private Rigidbody m_Rigidbody;

        private bool m_Connected;
        private int m_Index;

        private void Start()
        {
            m_Transform = transform;
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Ocean = OceanPhysics.Instance;

            m_Connected = m_Ocean.AddBody(transform, out m_Index);
        }

        private void FixedUpdate()
        {
            if(!m_Connected)
                return;

            _ = m_Ocean.GetData(m_Index, out var imp, out var up);
            _ = m_Ocean.UpdateBodyPosition(transform, m_Index);
            
            m_Rigidbody.AddForce(imp);
            if(m_AlignWithNormal)
                m_Transform.up = up;
        }
    }
}