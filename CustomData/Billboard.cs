namespace NNT_Archipealgo.CustomData
{
    internal class Billboard : MonoBehaviour
    {
        void LateUpdate()
        {
            transform.forward = Camera.main.transform.forward;
        }
    }
}
