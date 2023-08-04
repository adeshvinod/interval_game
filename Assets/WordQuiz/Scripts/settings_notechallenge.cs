using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class settings_notechallenge : MonoBehaviour
{
    public static settings_notechallenge instance; //Instance to make is available in other scripts without reference
    public Transform[] y_coordinates;
    public Transform[] x_coordinates;

    private int scalefactor_x = 1;
    private int scalefactor_y = 1;

    private Vector2 current_pos;

    private Vector2 currentScale;
    private int current_x_coord=0;
    private int current_y_coord=0;

    public float startTime;
    public Text timerText;

    public int current_level;

    Scene scene;

    public Dictionary<(int,int), int> Coordinate_system = new Dictionary<(int,int), int>()  //coordinate system of the fretboard
    {   {(0,0),0},
        {(0,1),1},
        {(0,2),2},
        {(0,3),3},
        {(0,4),4},
        {(0,5),5},
        {(0,6),6},
        {(0,7),7},
        {(0,8),8},
        {(0,9),9},
        {(0,10),10},
        {(0,11),11},
        {(0,12),12},

        {(1,0),13},
        {(1,1),14},
        {(1,2),15},
        {(1,3),16},
        {(1,4),17},
        {(1,5),18},
        {(1,6),19},
        {(1,7),20},
        {(1,8),21},
        {(1,9),22},
        {(1,10),23},
        {(1,11),24},
        {(1,12),25},

        {(2,0),26},
        {(2,1),27},
        {(2,2),28},
        {(2,3),29},
        {(2,4),30},
        {(2,5),31},
        {(2,6),32},
        {(2,7),33},
        {(2,8),34},
        {(2,9),35},
        {(2,10),36},
        {(2,11),37},
        {(2,12),38},

        {(3,0),39},
        {(3,1),40},
        {(3,2),41},
        {(3,3),42},
        {(3,4),43},
        {(3,5),44},
        {(3,6),45},
        {(3,7),46},
        {(3,8),47},
        {(3,9),48},
        {(3,10),49},
        {(3,11),50},
        {(3,12),51},

        {(4,0),52},
        {(4,1),53},
        {(4,2),54},
        {(4,3),55},
        {(4,4),56},
        {(4,5),57},
        {(4,6),58},
        {(4,7),59},
        {(4,8),60},
        {(4,9),61},
        {(4,10),62},
        {(4,11),63},
        {(4,12),64},

        {(5,0),65},
        {(5,1),66},
        {(5,2),67},
        {(5,3),68},
        {(5,4),69},
        {(5,5),70},
        {(5,6),71},
        {(5,7),72},
        {(5,8),73},
        {(5,9),74},
        {(5,10),75},
        {(5,11),76},
        {(5,12),77},

    };

    public List<(int,int)> questionList;

    private GameObject unmask_unit;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

    }
    // Start is called before the first frame update
    void Start()
    {
        // GameObject x_coordinates_parent = GameObject.Find("x_coordinates");
        // x_coordinates = x_coordinates_parent.GetComponentsInChildren<Transform>();


        //GameObject y_coordinates_parent = GameObject.Find("y_coordinates");
        // y_coordinates = y_coordinates_parent.GetComponentsInChildren<Transform>();

        scene = SceneManager.GetActiveScene();

        if (scene.name == "notes_settings")
        {

            unmask_unit = GameObject.Find("unmask_unit");
            currentScale = unmask_unit.transform.localScale;
            current_pos = unmask_unit.transform.localPosition;
        } 
        startTime = Time.time;
        questionList = new List<(int, int)>();

    }

    public void right_edge(int direction)
    {
        if (direction == 1 && scalefactor_x <= 12)
        {


            unmask_unit.GetComponent<Transform>().localScale = new Vector2((currentScale.x / scalefactor_x) * (scalefactor_x + 1), currentScale.y);
            //unmask_unit.GetComponent<Transform>().localPosition = initial_pos;
            scalefactor_x = scalefactor_x + 1;
            currentScale = unmask_unit.transform.localScale;


        }
        else if (direction == -1 && scalefactor_x>1)
        {
            unmask_unit.GetComponent<Transform>().localScale = new Vector2((currentScale.x / scalefactor_x) * (scalefactor_x - 1), currentScale.y);
            scalefactor_x = scalefactor_x - 1;
            currentScale = unmask_unit.transform.localScale;
        }

        Debug.Log("scale_x:" + scalefactor_x+"     scale_y:"+scalefactor_y);
        Debug.Log("x_pos:" + current_x_coord + "     y_pos:" + current_y_coord);
    }


    public void bottom_edge(int direction)
    {
        if(direction==1 && scalefactor_y<6)
        {
            unmask_unit.GetComponent<Transform>().localScale = new Vector2(currentScale.x, (currentScale.y/scalefactor_y)*(scalefactor_y+1));
            scalefactor_y = scalefactor_y + 1;
            currentScale = unmask_unit.transform.localScale;
        }
        else if (direction == -1 && scalefactor_y>1)
        {
            unmask_unit.GetComponent<Transform>().localScale = new Vector2(currentScale.x, (currentScale.y / scalefactor_y) * (scalefactor_y - 1));
            scalefactor_y = scalefactor_y - 1;
            currentScale = unmask_unit.transform.localScale;
        }
        Debug.Log("scale_x:" + scalefactor_x + "     scale_y:" + scalefactor_y);
        Debug.Log("x_pos:" + current_x_coord + "     y_pos:" + current_y_coord);
    }

    public void left_edge(int direction)
    {
        if(direction==1 && scalefactor_x>1)
        {
            current_x_coord = current_x_coord+1;
            unmask_unit.GetComponent<Transform>().localPosition = new Vector2(x_coordinates[current_x_coord].localPosition.x,y_coordinates[current_y_coord].localPosition.y);
            unmask_unit.GetComponent<Transform>().localScale = new Vector2((currentScale.x / scalefactor_x) * (scalefactor_x - 1), currentScale.y);
            scalefactor_x = scalefactor_x - 1;
            currentScale = unmask_unit.transform.localScale;
        }
        else if(direction==-1 && current_x_coord>0)
        {
            current_x_coord = current_x_coord - 1;
            unmask_unit.GetComponent<Transform>().localPosition = new Vector2(x_coordinates[current_x_coord].localPosition.x, y_coordinates[current_y_coord].localPosition.y);
            unmask_unit.GetComponent<Transform>().localScale = new Vector2((currentScale.x / scalefactor_x) * (scalefactor_x + 1), currentScale.y);
            scalefactor_x = scalefactor_x + 1;
            currentScale = unmask_unit.transform.localScale;

        }
        Debug.Log("scale_x:" + scalefactor_x + "     scale_y:" + scalefactor_y);
        Debug.Log("x_pos:" + current_x_coord + "     y_pos:" + current_y_coord);
    }


    public void top_edge(int direction)
    {
        if(direction==-1 && scalefactor_y>1)//move downwards
        {
            current_y_coord = current_y_coord + 1;
            unmask_unit.GetComponent<Transform>().localPosition = new Vector2(x_coordinates[current_x_coord].localPosition.x, y_coordinates[current_y_coord].localPosition.y);
            unmask_unit.GetComponent<Transform>().localScale = new Vector2(currentScale.x, (currentScale.y / scalefactor_y) * (scalefactor_y - 1));
            scalefactor_y = scalefactor_y - 1;
            currentScale = unmask_unit.transform.localScale;
        }
        else if(direction==1 && current_y_coord>0)
        {
            current_y_coord = current_y_coord - 1;
            unmask_unit.GetComponent<Transform>().localPosition = new Vector2(x_coordinates[current_x_coord].localPosition.x, y_coordinates[current_y_coord].localPosition.y);
            unmask_unit.GetComponent<Transform>().localScale = new Vector2(currentScale.x, (currentScale.y / scalefactor_y) * (scalefactor_y + 1));
            scalefactor_y = scalefactor_y + 1;
            currentScale = unmask_unit.transform.localScale;
        }
        Debug.Log("scale_x:" + scalefactor_x + "     scale_y:" + scalefactor_y);
        Debug.Log("x_pos:" + current_x_coord + "     y_pos:" + current_y_coord);
    }



    // Update is called once per frame
    void Update()
    {

        
        //float t = Time.time - startTime;
        // string minutes = ((int)t / 60).ToString();
        //string seconds = (t % 60).ToString("f2");
        //timerText.text = minutes + ":" + seconds;
        if (scene.buildIndex == 6)
        {
            questionList.Clear();

            for (int j = current_y_coord; j < current_y_coord + scalefactor_y; j++)
            {
                for (int i = current_x_coord; i < current_x_coord + scalefactor_x; i++)
                {
                    questionList.Add((j, i));

                }
            }
        }

    }

     public void level_select(int level)
    {
        questionList.Clear();
        
        switch (level)
        {
            case 1:
                current_level = 1;
                for (int i = 4;i<=5;i++)
                {
                    for(int j=0;j<=12;j++)
                    {
                        questionList.Add((i, j));
                    }

                }
                         
                        break;

            case 2:
                current_level = 2;
                for (int i = 0; i <= 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        questionList.Add((i, j));
                    }

                }



                break;

            case 3:
                current_level = 3;
                for (int i = 0; i <= 5; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        questionList.Add((i, j));
                    }

                }


                break;

            case 4:
                current_level = 4;
                for (int i = 0; i <= 5; i++)
                {
                    for (int j = 0; j <= 12; j++)
                    {
                        questionList.Add((i, j));
                    }

                }



                break;

            default:   
                        break;
        }
    }
}
