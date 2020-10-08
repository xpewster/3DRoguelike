using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLevel : MonoBehaviour
{
    public Transform playerTransform;

    public GameObject[] rooms;
    public GameObject hallwaySegment;

    public LayerMask roomMask;

    List<GameObject> level;
    // Start is called before the first frame update
    void Start()
    {
        level = new List<GameObject>(); 
        level.Add(PlaceRoom(1, new Vector3(0f, 0f, 0f), 0, -1));

        Generate(level);
        
    }

    // Generate the map. level should contain only the start room
    void Generate(List<GameObject> level)
    {
        GameObject startRoom = level[0];
        // for each deadend:
        for (int j = 0; j < 3; j++)
        {
            int DE_RoomCount = Random.Range(5,8);
            int roomID = Random.Range(0,3);
            int direction = Random.Range(0,4); // Clockwise from z+ = 0
            GameObject currentRoom = startRoom;
            while (DE_RoomCount > 0)
            {
                List<GameObject> doors = GetDoorsDirected(currentRoom,direction);
                GameObject exitDoor = doors[Random.Range(0,doors.Count)];

                int hallwayLength = Random.Range(4,10);
                int roomRot = Random.Range(0,4);

                int correctedDir = (direction + 6 - roomRot) % 4;
                List<GameObject> entryDoors = GetDoorsDirected(rooms[roomID], correctedDir); 
                int entryDoorNum = Random.Range(0,entryDoors.Count);
                GameObject entryDoor = entryDoors[entryDoorNum];
                entryDoorNum += correctedDir*10;

                //Debug.Log("Dir: "+direction+" , Roomrot: "+roomRot+" , Ex: "+exitDoor.name+" , Ent: "+entryDoor.name + " "+entryDoorNum);

                Vector3 dimensions = rooms[roomID].transform.Find("Floor").localScale;

                Debug.Log(rooms[roomID].transform.localScale*5 + Vector3.up * 50);

                Vector3 roomPosition = exitDoor.transform.position + hallwayLength * hallwaySegment.transform.Find("Floor").localScale.z*10 * VectorDirected(direction) - RotateAround(entryDoor.transform.localPosition, new Vector3 (0f, 0f, 0f), Quaternion.Euler(0f, roomRot * 90f, 0f));
                if (Physics.OverlapBox(roomPosition, rooms[roomID].transform.localScale*5 + Vector3.up * 50, Quaternion.Euler(0f, roomRot * 90f, 0f), roomMask, QueryTriggerInteraction.Ignore).Length == 0
                        && !Physics.BoxCast(exitDoor.transform.position + (hallwaySegment.transform.Find("Floor").lossyScale.z*6*VectorDirected(direction)), hallwaySegment.transform.lossyScale + Vector3.up * 50,VectorDirected(direction),
                            Quaternion.Euler(0f, direction * 90f, 0f), hallwayLength * hallwaySegment.transform.Find("Floor").lossyScale.z*10, roomMask, QueryTriggerInteraction.Ignore))
                {     
                    // Place hallway
                    Vector3 offset = (hallwaySegment.transform.Find("Floor").lossyScale.z*5*VectorDirected(direction));
                    for (int i = 0; i < hallwayLength; i++)
                    {
                        Instantiate(hallwaySegment, exitDoor.transform.position + offset + i * hallwaySegment.transform.Find("Floor").lossyScale.z*10 * VectorDirected(direction), Quaternion.Euler(0f, direction * 90f, 0f));
                    }
                    // Place room and open door
                    currentRoom = PlaceRoom(roomID, roomPosition, roomRot, entryDoorNum);
                    exitDoor.GetComponent<MeshRenderer>().enabled = false;
                    exitDoor.GetComponent<BoxCollider>().enabled = false;
                    level.Add(currentRoom);
                }
                roomID = Random.Range(0,3);
                direction = Random.Range(0,4);
                DE_RoomCount--;
            }
        }
    }

    GameObject PlaceRoom(int roomID, Vector3 pos, int rot, int entryDoorNum)
    {
        GameObject room = Instantiate(rooms[roomID], pos, Quaternion.Euler(0f, rot*90f, 0f));

        List<GameObject> doors = GetDoors(room);

        // Find and disable the door way the hallway is entering into
        GameObject entryDoor;
        if (entryDoorNum >= 0 && entryDoorNum < 10)
            entryDoor = doors.Find(obj => (obj.name == "Door0"+entryDoorNum));
        else
            entryDoor = doors.Find(obj => (obj.name == "Door"+entryDoorNum));
        if (entryDoor != null)
        {
            
            entryDoor.GetComponent<MeshRenderer>().enabled = false;
            entryDoor.GetComponent<BoxCollider>().enabled = false;
        }
        
        return room;
    }

    // Fetches the door game objects into a list
    List<GameObject> GetDoors(GameObject room)
    {
        List<GameObject> doors = new List<GameObject>();

        foreach (Transform child in room.transform)
        {
            if (child.tag == "Door")
            {
                doors.Add(child.gameObject);
            }
        }

        return doors;
    }

    // Fetches the door game objects that are going towards a certain direction. Direction is determined by the first number digit of the door name.
    List<GameObject> GetDoorsDirected(GameObject room, int direction)
    {
        List<GameObject> doorsDirected = new List<GameObject>();
        List<GameObject> doors = GetDoors(room);

        //Adjust for room rotation
        int y = (int)(room.transform.rotation.eulerAngles.y/90f);

        foreach(GameObject door in doors)
        {
            if (door.name[4] == ((direction+4-y) % 4)+48)
            {
                doorsDirected.Add(door);
            }
        }

        return doorsDirected;
    }

    // Convert direction to a directed unit vector
    Vector3 VectorDirected(int direction)
    {
        Quaternion rotation = Quaternion.Euler(0f, direction * 90f, 0f);
        return rotation * Vector3.forward;
    }

    // Rotate point around a pivot
    Vector3 RotateAround(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        Vector3 dir = point - pivot;
        dir = rotation * dir;
        return pivot + dir;
    }
}
