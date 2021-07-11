using UnityEngine;

public class ExplodeCube : MonoBehaviour
{
    public GameObject restartButton, explosion;
    private bool _collisionSet;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Cube" && !_collisionSet)
        {
            for (int i = collision.transform.childCount - 1; i >= 0; i--)
            {
                Transform Child = collision.transform.GetChild(i);
                Child.gameObject.AddComponent<Rigidbody>();
                Child.gameObject.GetComponent<Rigidbody>().AddExplosionForce(70f, Vector3.up, 5f);
                Child.SetParent(null);
            }
            restartButton.SetActive(true);
            Camera.main.gameObject.transform.position -= new Vector3(0, 0, 3f);
            Camera.main.gameObject.AddComponent<CameraShake>();

            GameObject newVfx = Instantiate(explosion,
                new Vector3(collision.contacts[0].point.x, collision.contacts[0].point.y, collision.contacts[0].point.z),
                Quaternion.identity) as GameObject;

            Destroy(newVfx, 1f);

            if (PlayerPrefs.GetString("music") != "No")
                GetComponent<AudioSource>().Play();

            Destroy(collision.gameObject);
            _collisionSet = true;
        }
    }
}
