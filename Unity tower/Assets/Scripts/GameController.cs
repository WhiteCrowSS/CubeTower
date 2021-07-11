using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private CubePos nowCube = new CubePos(0, 1, 0);
    public float cubeChangePlaceSpeed = 0.5f;
    public Transform cubeToPlace;
    public GameObject allCubes, vfx;
    private Rigidbody allcubesRB;
    public GameObject[] canvasStartPage;
    public GameObject[] cubesToCreate;
    public Text scoreTxt;
    private bool IsLoose, firstCube;
    private float camMoveToYPos, camMoveSpeed = 2f;
    private int prevCountMaxHor;
    private List<Vector3> allCubesPositions = new List<Vector3>
    {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, 0, 1),
        new Vector3(0, 0, -1),
        new Vector3(1, 0, 1),
        new Vector3(-1, 0, -1),
        new Vector3(-1, 0, 1),
        new Vector3(1, 0, -1)
    };

    private Transform mainCam;
    private Coroutine show_cube_place;
    public Color[] bgColor;
    private Color toColorChange;

    private List<GameObject> posibleCubesToCreate = new List<GameObject>();

    private void Start()
    {
        GetUnlockedCubesToCreate();
        toColorChange = Camera.main.backgroundColor;
        mainCam = Camera.main.transform;
        camMoveToYPos = 7f + nowCube.y - 1f;
        allcubesRB = allCubes.GetComponent<Rigidbody>(); 
        show_cube_place = StartCoroutine(ShowCubePlace());
    }

    private void Update()
    {
        if ((Input.GetMouseButtonDown(0) || Input.touchCount > 0) && cubeToPlace != null && allCubes != null && !EventSystem.current.IsPointerOverGameObject())
        {
#if !UNITY_EDITOR
            if (Input.GetTouch(0).phase != TouchPhase.Began)
                return;
#endif

            
            if(!firstCube)
            {
                scoreTxt.gameObject.SetActive(true);
                firstCube = true;
                foreach (GameObject item in canvasStartPage)
                {
                    Destroy(item);
                }
            }

            GameObject createCube = null;
            if (posibleCubesToCreate.Count == 1)
                createCube = posibleCubesToCreate[0];
            else
                createCube = posibleCubesToCreate[UnityEngine.Random.Range(0, posibleCubesToCreate.Count)];

            GameObject newCube = Instantiate(
                createCube,
                cubeToPlace.position,
                Quaternion.identity) as GameObject;

            newCube.transform.SetParent(allCubes.transform);
            nowCube.setVector(cubeToPlace.position);
            allCubesPositions.Add(nowCube.getVector());

            if (PlayerPrefs.GetString("music") != "No")
                GetComponent<AudioSource>().Play();

            GameObject newVfx = Instantiate(vfx, newCube.transform.position, Quaternion.identity) as GameObject;
            Destroy(newVfx, 1.5f);

            allcubesRB.isKinematic = true;
            allcubesRB.isKinematic = false;

            SpawnPositions();
            MoveCameraChangeBG();
        }

        if (!IsLoose && allcubesRB.velocity.magnitude > 0.1f)
        {
            Destroy(cubeToPlace.gameObject);
            IsLoose = true;
            StopCoroutine(show_cube_place);
        }

        mainCam.localPosition = Vector3.MoveTowards(mainCam.localPosition,
            new Vector3(mainCam.localPosition.x, camMoveToYPos, mainCam.localPosition.z), 
            camMoveSpeed * Time.deltaTime);

        if (Camera.main.backgroundColor != toColorChange)
            Camera.main.backgroundColor = Color.Lerp(Camera.main.backgroundColor, toColorChange, Time.deltaTime / 1.5f);
    }

    IEnumerator ShowCubePlace()
    {
        while (true)
        {
            SpawnPositions();

            yield return new WaitForSeconds(cubeChangePlaceSpeed);
        }
    }

    private void SpawnPositions()
    {
        List<Vector3> positions = new List<Vector3>();

        if (IsPositionEmpty(new Vector3(nowCube.x + 1, nowCube.y, nowCube.z)) && nowCube.x + 1 != cubeToPlace.position.x)
            positions.Add(new Vector3(nowCube.x + 1, nowCube.y, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x - 1, nowCube.y, nowCube.z)) && nowCube.x - 1 != cubeToPlace.position.x)
            positions.Add(new Vector3(nowCube.x - 1, nowCube.y, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y + 1, nowCube.z)) && nowCube.y + 1 != cubeToPlace.position.y)
            positions.Add(new Vector3(nowCube.x, nowCube.y + 1, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y - 1, nowCube.z)) && nowCube.y - 1 != cubeToPlace.position.y)
            positions.Add(new Vector3(nowCube.x, nowCube.y - 1, nowCube.z));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y, nowCube.z + 1)) && nowCube.z + 1 != cubeToPlace.position.z)
            positions.Add(new Vector3(nowCube.x, nowCube.y, nowCube.z + 1));
        if (IsPositionEmpty(new Vector3(nowCube.x, nowCube.y, nowCube.z - 1)) && nowCube.z - 1 != cubeToPlace.position.z)
            positions.Add(new Vector3(nowCube.x, nowCube.y, nowCube.z - 1));

        if (positions.Count > 1)
            cubeToPlace.position = positions[UnityEngine.Random.Range(0, positions.Count)];
        else if (positions.Count == 0)
            IsLoose = true; 
        else
            cubeToPlace.position = positions[0];        
    }

    private bool IsPositionEmpty(Vector3 targetpos)
    {
        if (targetpos.y == 0)
            return false;

        foreach (Vector3 pos in allCubesPositions)
        {
            if (pos.x == targetpos.x && pos.y == targetpos.y && pos.z == targetpos.z)
                return false;
        }

        return true;
    }

    private void MoveCameraChangeBG()
    {
        int maxX = 0, maxY = 0, maxZ = 0, maxHor;

        foreach (Vector3 item in allCubesPositions)
        {
            if (Mathf.Abs(Convert.ToInt32(item.x)) > maxX)
                maxX = Convert.ToInt32(item.x);

            if (Convert.ToInt32(item.y) > maxY)
                maxY = Convert.ToInt32(item.y);

            if (Mathf.Abs(Convert.ToInt32(item.z)) > maxZ)
                maxZ = Convert.ToInt32(item.z);
        }

        maxY--;
        if (PlayerPrefs.GetInt("score") < maxY)
            PlayerPrefs.SetInt("score", maxY);

        scoreTxt.text = "<size=35><color=#BD4E46>Best </color></size> " + PlayerPrefs.GetInt("score")
            + "<size=25> Score </size> " + maxY;

        camMoveToYPos = 7f + nowCube.y - 1f;

        maxHor = maxX > maxZ ? maxX : maxZ;
        if (maxHor % 3 == 0 && prevCountMaxHor != maxHor)
        {
            mainCam.localPosition = new Vector3(0, 0, 2.5f);
            prevCountMaxHor = maxHor;
        }

        if (maxY >= 20)
            toColorChange = bgColor[2];
        else if (maxY >= 10)
            toColorChange = bgColor[1];
        else if (maxY >= 5)
            toColorChange = bgColor[0];
    }

    private void AddPosibleCubes(int till)
    {
        for (int i = 0; i < till; i++)  
            posibleCubesToCreate.Add(cubesToCreate[i]);
    }

    private void GetUnlockedCubesToCreate()
    {
        if (PlayerPrefs.GetInt("score") < 5)
            posibleCubesToCreate.Add(cubesToCreate[0]);
        else if (PlayerPrefs.GetInt("score") < 10)
            AddPosibleCubes(2);
        else if (PlayerPrefs.GetInt("score") < 20)
            AddPosibleCubes(3);
        else if (PlayerPrefs.GetInt("score") < 25)
            AddPosibleCubes(4);
        else if (PlayerPrefs.GetInt("score") < 30)
            AddPosibleCubes(5);
        else if (PlayerPrefs.GetInt("score") < 40)
            AddPosibleCubes(6);
        else if (PlayerPrefs.GetInt("score") < 50)
            AddPosibleCubes(7);
        else if (PlayerPrefs.GetInt("score") < 60)
            AddPosibleCubes(8);
        else if (PlayerPrefs.GetInt("score") < 70)
            AddPosibleCubes(9);
        else
            AddPosibleCubes(10);
    }
}



struct CubePos
{
    public int x, y, z;
    public CubePos(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 getVector()
    {
        return new Vector3(x, y, z);
    }

    public void setVector(Vector3 pos)
    {
        x = Convert.ToInt32(pos.x);
        y = Convert.ToInt32(pos.y);
        z = Convert.ToInt32(pos.z);
    }
}
