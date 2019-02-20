using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using KModkit;
using UnityEngine;
using Random = UnityEngine.Random;

public class Megaman2 : MonoBehaviour
{

    public KMAudio Audio;
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    public KMSelectable[] GridPoints;
    public TextMesh Points;
    public GameObject Cursor;
    public GameObject Point;

    public Material[] RobotMastersMat;
    public Material[] WeaponsMat;

    public GameObject RobotMasters;
    public GameObject Weapons;

    private GameObject lastCursor;
    private GameObject lastPoint;

    private int selectedMaster;
    private int selectedWeapon;

    private int Time;

    private KMSelectable[][] Grid;
    private int PointsLeft = 9;

    private bool[][] calculatedPattern = new[]
    {
        new[] {false, false, false, false, false},
        new[] {false, false, false, false, false},
        new[] {false, false, false, false, false},
        new[] {false, false, false, false, false},
        new[] {false, false, false, false, false},
    };

    private bool Started = false;


    void Awake()
    {
        Grid = new[]
        {
            new[] { GridPoints[0], GridPoints[1], GridPoints[2], GridPoints[3], GridPoints[4] },
            new[] { GridPoints[5], GridPoints[6], GridPoints[7], GridPoints[8], GridPoints[9] },
            new[] { GridPoints[10], GridPoints[11], GridPoints[12], GridPoints[13], GridPoints[14] },
            new[] { GridPoints[15], GridPoints[16], GridPoints[17], GridPoints[18], GridPoints[19] },
            new[] { GridPoints[20], GridPoints[21], GridPoints[22], GridPoints[23], GridPoints[24] }
        };

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Grid[i][j].OnInteract += GridPress(i, j);
            }
        }

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Grid[i][j].OnSelect += GridSelect(i, j);
            }
        }

        BombModule.GetComponent<KMSelectable>().OnInteract += delegate 
        {
            if (!Started)
                Audio.PlaySoundAtTransform("Start", transform);
            Started = true;
            return true;
        };

    }

    private KMSelectable.OnInteractHandler GridPress(int row, int col)
    {
        return delegate ()
        {
            if (moduleSolved)
                return false;
            if (calculatedPattern[row][col])
            {
                calculatedPattern[row][col] = false;

                Audio.PlaySoundAtTransform("Click", transform);

                lastPoint = Instantiate(Point);
                lastPoint.transform.parent = Grid[row][col].transform;
                lastPoint.transform.localPosition = new Vector3(0, 0, 0);
                lastPoint.transform.localRotation = Quaternion.identity;
                lastPoint.transform.localScale = new Vector3(1, 1, 1);

                PointsLeft--;
                Points.text = PointsLeft.ToString();

                if (PointsLeft == 0)
                {
                    StartCoroutine(Solve());
                }
                return false;
            }
            else
            {
                Audio.PlaySoundAtTransform("Strike", transform);
                BombModule.HandleStrike();
                return false;
            }

        };
    }

    private IEnumerator Solve()
    {
        BombModule.HandlePass();
        yield return new WaitForSeconds(0.5f);
        Audio.PlaySoundAtTransform("Solve", transform);
        moduleSolved = true;
    }

    private Action GridSelect(int row, int col)
    {
        return delegate ()
        {
            if(lastCursor != null)
                Destroy(lastCursor);
            lastCursor = Instantiate(Cursor);
            lastCursor.transform.parent = Grid[row][col].transform;
            lastCursor.transform.localPosition = new Vector3(0, 0, 0);
            lastCursor.transform.localRotation = Quaternion.identity;
            lastCursor.transform.localScale = new Vector3(1, 1, 1);
        };
    }

    void Start()
    {
        Time = (int)BombInfo.GetTime();
        moduleId = moduleIdCounter++;
        Points.text = PointsLeft.ToString();

        selectedMaster = Random.Range(0, RobotMastersMat.Length);
        selectedWeapon = Random.Range(0, WeaponsMat.Length);

        RobotMasters.GetComponent<MeshRenderer>().material = RobotMastersMat[selectedMaster];
        Weapons.GetComponent<MeshRenderer>().material = WeaponsMat[selectedWeapon];

        CalculatePattern();

    }

    void CalculatePattern()
    {

        int ETank = BombInfo.GetSerialNumberNumbers().Sum();

        while (ETank > 5)
            ETank -= 5;

        switch (ETank)
        {
            case 1:
                calculatedPattern[0][0] = true;
                break;
            case 2:
                calculatedPattern[0][1] = true;
                break;
            case 3:
                calculatedPattern[0][2] = true;
                break;
            case 4:
                calculatedPattern[0][3] = true;
                break;
            case 5:
                calculatedPattern[0][4] = true;
                break;
        }

        switch (selectedMaster)
        {
            case 0:
                calculatedPattern[4][0] = true;
                break;
            case 1:
                calculatedPattern[3][1] = true;
                break;
            case 2:
                calculatedPattern[1][0] = true;
                break;
            case 3:
                calculatedPattern[1][2] = true;
                break;
            case 4:
                calculatedPattern[4][3] = true;
                break;
            case 5:
                calculatedPattern[4][4] = true;
                break;
            case 6:
                calculatedPattern[3][2] = true;
                break;
            case 7:
                calculatedPattern[2][3] = true;
                break;
        }

        switch (selectedWeapon)
        {
            case 0:
                calculatedPattern[1][1] = true;
                break;
            case 1:
                calculatedPattern[3][4] = true;
                break;
            case 2:
                calculatedPattern[3][3] = true;
                break;
            case 3:
                calculatedPattern[2][4] = true;
                break;
            case 4:
                calculatedPattern[2][0] = true;
                break;
            case 5:
                calculatedPattern[1][3] = true;
                break;
            case 6:
                calculatedPattern[2][2] = true;
                break;
            case 7:
                calculatedPattern[4][1] = true;
                break;
        }



        var remainingRobotMasters = Enumerable.Range(0, RobotMastersMat.Length).Except(new[] { selectedMaster, selectedWeapon }).ToArray();

        for (int i = 0; i < remainingRobotMasters.Length; i++)
        {
            switch (remainingRobotMasters[i])
            {
                case 0:
                    if (BombInfo.GetBatteryHolderCount() >= BombInfo.GetIndicators().Count())
                        calculatedPattern[4][0] = true;
                    else
                        calculatedPattern[1][1] = true;
                    break;
                case 1:
                    if (ETank < 2 || ETank > 2)
                        calculatedPattern[3][1] = true;
                    else
                        calculatedPattern[3][4] = true;
                    break;
                case 2:
                    if (BombInfo.IsIndicatorPresent(Indicator.CAR))
                        calculatedPattern[1][0] = true;
                    else
                        calculatedPattern[3][3] = true;
                    break;
                case 3:
                    if (BombInfo.GetSerialNumberNumbers().Last() <= 5)
                        calculatedPattern[1][2] = true;
                    else
                        calculatedPattern[2][4] = true;
                    break;
                case 4:
                    if (BombInfo.GetSolvableModuleNames().Count() >= 11)
                        calculatedPattern[4][3] = true;
                    else
                        calculatedPattern[2][0] = true;
                    break;
                case 5:
                    if (BombInfo.GetBatteryCount() == BombInfo.GetSerialNumberNumbers().First())
                        calculatedPattern[4][4] = true;
                    else
                        calculatedPattern[1][3] = true;
                    break;
                case 6:
                    if (BombInfo.IsPortPresent(Port.RJ45))
                        calculatedPattern[3][2] = true;
                    else
                        calculatedPattern[2][2] = true;
                    break;
                case 7:
                    if (Time >= 2400)
                        calculatedPattern[2][3] = true;
                    else
                        calculatedPattern[4][1] = true;
                    break;
            }
        }

        Debug.LogFormat(@"[Megaman 2 #{0}] Password is: {1}", moduleId, Enumerable.Range(0, 5).SelectMany(row => Enumerable.Range(0, 5).Where(col => calculatedPattern[row][col]).Select(col => (char)('A' + row) + (col + 1).ToString())).Join(", "));
    }


}
