using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DayNightSystem : MonoBehaviour
{
    public float currentTime; 
    public float dayLenghtMinutes;
    public TextMeshProUGUI timeText;
    private EnemySpawnController spawnController;

    public bool isDay;

    //public Material stars;

    //private float rotationSpeed;
    private float midDay;
/*    private float morning = 9f;
    private float evening = 9f;
    private bool isMorning;
    private bool isEvening;*/
    private float translateTime;
    string amPm = "AM";
    
    public bool Isday
    {
        get { return isDay; }
    }
    void Start()
    {
        //rotationSpeed = 360 / dayLenghtMinutes / 61;
        midDay = dayLenghtMinutes * 60 / 2;
    }


    void Update()
    {
        currentTime += 1 * Time.deltaTime;
        translateTime = (currentTime / (midDay * 2));

        float t = translateTime * 24f;

        float hours = Mathf.Floor(t);

        string displayHours = hours.ToString();

        if(hours == 0)
        {
            displayHours = "12";
        }
        if(hours > 12)
        {
            displayHours = (hours - 12).ToString();
        }
        //Strj?rnor
       /* if(currentTime >= midDay / 2 && currentTime  <= midDay * 1.5f)
        {
            if (stars.GetFloat("_Cutoff") < 1)
            {
                float alpha = stars.GetFloat("_Cutoff") * 100f;
                alpha += 3 * rotationSpeed * Time.deltaTime;
                alpha = alpha * .01f;
                stars.SetFloat("_Cutoff", alpha);
            }
        }
        else
        {
            if (stars.GetFloat("_Cutoff") > .2f)
            {
                float alpha = stars.GetFloat("_Cutoff") * 100f;
                alpha -= 3 * rotationSpeed * Time.deltaTime;
                alpha = alpha * .01f;
                stars.SetFloat("_Cutoff", alpha);
            }
        }*/
        //AMPM
        if(currentTime >= midDay)
        {
            if(amPm != "PM")
            {
                amPm = "PM";
            }
        }
        if(currentTime >= midDay * 2)
        {
            if(amPm != "AM")
            {
                amPm = "AM";
            }
            currentTime = 0;
        }

        if(currentTime >= midDay / 2 && currentTime <= midDay * 1.5)
        {
            isDay = true;
            //Debug.Log(isDay);
        }
        
        if(currentTime >= midDay * 1.5)
        {
            isDay = false;
            //Debug.Log(isDay);
        }


        //Minuter
        t *= 60;
        float minutes = Mathf.Floor(t % 60);

        string displayMinutes = minutes.ToString();
        if(minutes < 10)
        {
            displayMinutes = "0" + minutes.ToString();
        }

        string displayTime = displayHours + ":" + displayMinutes + " " + amPm;

        timeText.text = displayTime;
        
        //transform.Rotate(new Vector3(1, 0, 0) * rotationSpeed * Time.deltaTime);
    }
}
