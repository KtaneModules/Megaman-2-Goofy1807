using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KModkit;
using UnityEngine;
using Random = UnityEngine.Random;

public class Megaman2 : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMRuleSeedable RuleSeedable;

    public KMSelectable ModuleSelectable;
    public KMSelectable[] GridPoints;
    public TextMesh Points;
    public GameObject Cursor;
    public GameObject Point;

    public Texture[] RobotMasters;
    public Texture[] Weapons;
    public Texture Border;
    public Texture Empty;

    public MeshRenderer RobotMastersDisplay;
    public MeshRenderer WeaponsDisplay;

    private GameObject lastCursor;
    private GameObject lastPoint;

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved;
    private string[] robotMasters;
    private int selectedMaster;
    private int selectedWeapon;
    private int time, day, month;
    private KMSelectable[][] grid;
    private bool started = false;
    private string[] solution;
    private readonly List<string> pressed = new List<string>();

    void Awake()
    {
        grid = new[]
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
                grid[i][j].OnInteract += GridPress(i, j);
                grid[i][j].OnSelect += GridSelect(i, j);
            }
        }

        ModuleSelectable.OnInteract += delegate
        {
            if (!started)
                Audio.PlaySoundAtTransform("Start" + Random.Range(1, 5), transform);
            started = true;
            return true;
        };
    }

    private KMSelectable.OnInteractHandler GridPress(int row, int col)
    {
        return delegate ()
        {
            grid[row][col].AddInteractionPunch();

            var coord = ((char) ('A' + row)) + "" + (col + 1);

            if (moduleSolved || pressed.Contains(coord))
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                return false;
            }

            if (!solution.Contains(coord))
            {
                Audio.PlaySoundAtTransform("Strike", transform);
                BombModule.HandleStrike();
                return false;
            }

            Audio.PlaySoundAtTransform("Click", transform);
            lastPoint = Instantiate(Point);
            lastPoint.transform.parent = grid[row][col].transform;
            lastPoint.transform.localPosition = new Vector3(0, 0, 0);
            lastPoint.transform.localRotation = Quaternion.identity;
            lastPoint.transform.localScale = new Vector3(1, 1, 1);

            pressed.Add(coord);
            Points.text = (solution.Length - pressed.Count).ToString();

            if (pressed.Count == solution.Length)
                StartCoroutine(Solve());
            return false;
        };
    }

    private IEnumerator Solve()
    {
        BombModule.HandlePass();
        yield return new WaitForSeconds(0.5f);
        Audio.PlaySoundAtTransform("Solve" + Random.Range(1, 5), transform);
        moduleSolved = true;
        RobotMastersDisplay.material.mainTexture = Border;
        WeaponsDisplay.material.mainTexture = Empty;

    }

    private Action GridSelect(int row, int col)
    {
        return delegate ()
        {
            if (lastCursor != null)
                Destroy(lastCursor);
            lastCursor = Instantiate(Cursor);
            lastCursor.transform.parent = grid[row][col].transform;
            lastCursor.transform.localPosition = new Vector3(0, 0, 0);
            lastCursor.transform.localRotation = Quaternion.identity;
            lastCursor.transform.localScale = new Vector3(1, 1, 1);
        };
    }

    sealed class Coords
    {
        public string DeadCoord { get; private set; }
        public string AliveCoord { get; private set; }
        public Coords(string deadCoord, string aliveCoord)
        {
            DeadCoord = deadCoord;
            AliveCoord = aliveCoord;
        }
    }

    sealed class Rule
    {
        public string Text { get; private set; }

        // The ‘int’ parameter is the ETanks spot column
        public Func<int, bool> Evaluate { get; private set; }

        public bool ETanks { get; private set; }
        public Rule(string text, Func<int, bool> evaluate, bool eTanks = false)
        {
            Text = text;
            Evaluate = evaluate;
            ETanks = eTanks;
        }
    }

    sealed class EdgeworkRule
    {
        public string Name { get; private set; }
        public int Value { get; private set; }
        public EdgeworkRule(string name, int value)
        {
            Name = name;
            Value = value;
        }
    }

    static T[] newArray<T>(params T[] array) { return array; }

    void Start()
    {
        moduleId = moduleIdCounter++;
        time = (int) BombInfo.GetTime();
        day = DateTime.Now.Day;
        month = DateTime.Now.Month;

        // ** RULE SEED ** //
        var rnd = RuleSeedable.GetRNG();
        Debug.LogFormat(@"[Mega Man 2 #{0}] Using rule seed: {1}", moduleId, rnd.Seed);
        robotMasters = rnd.Seed == 1
            ? new[] { "Air Man", "Bubble Man", "Crash Man", "Flash Man", "Heat Man", "Metal Man", "Quick Man", "Wood Man" }
            : rnd.ShuffleFisherYates(new[] { "Cold Man", "Magma Man", "Dust Man", "Sword Man", "Splash Woman", "Ice Man", "Quick Man", "Hard Man", "Pharaoh Man", "Charge Man", "Pirate Man", "Pump Man", "Galaxy Man", "Grenade Man", "Snake Man", "Burst Man", "Cut Man", "Air Man", "Magnet Man", "Toad Man", "Gyro Man", "Tomahawk Man", "Wood Man", "Strike Man", "Blade Man", "Aqua Man", "Shade Man", "Flash Man", "Flame Man", "Concrete Man", "Metal Man", "Needle Man", "Wave Man", "Knight Man", "Slash Man", "Shadow Man", "Sheep Man", "Ground Man", "Wind Man", "Fire Man", "Stone Man", "Tengu Man", "Bright Man", "Centaur Man", "Cloud Man", "Frost Man", "Dynamo Man", "Chill Man", "Turbo Man", "Napalm Man", "Jewel Man", "Drill Man", "Freeze Man", "Blizzard Man", "Gravity Man", "Junk Man", "Clown Man", "Hornet Man", "Skull Man", "Solar Man", "Commando Man", "Yamato Man", "Dive Man", "Search Man", "Gemini Man", "Bubble Man", "Guts Man", "Tornado Man", "Astro Man", "Plug Man", "Elec Man", "Crystal Man", "Nitro Man", "Burner Man", "Spark Man", "Spring Man", "Plant Man", "Star Man", "Ring Man", "Top Man", "Crash Man", "Bomb Man", "Heat Man", "Magic Man" });

        var presetCoordinates = new Dictionary<string, Coords>
        {
            { "Air Man",    new Coords("B2", "E1") },
            { "Bubble Man", new Coords("D5", "D2") },
            { "Crash Man",  new Coords("D4", "B1") },
            { "Flash Man",  new Coords("C5", "B3") },
            { "Heat Man",   new Coords("C1", "E4") },
            { "Metal Man",  new Coords("B4", "E5") },
            { "Quick Man",  new Coords("C3", "D3") },
            { "Wood Man",   new Coords("E2", "C4") }
        };

        var availableCoordinates = new List<string>();
        for (var row = 1; row < 5; row++)
            for (var col = 0; col < 5; col++)
                availableCoordinates.Add((char) ('A' + row) + "" + (col + 1));
        for (int i = 0; i < 8; i++)
        {
            if (presetCoordinates.ContainsKey(robotMasters[i]))
            {
                availableCoordinates.Remove(presetCoordinates[robotMasters[i]].DeadCoord);
                availableCoordinates.Remove(presetCoordinates[robotMasters[i]].AliveCoord);
            }
        }

        rnd.ShuffleFisherYates(availableCoordinates);

        var coordinatesIx = 0;
        var coordinates = new Coords[8];

        for (var i = 0; i < 8; i++)
        {
            if (presetCoordinates.ContainsKey(robotMasters[i]))
                coordinates[i] = presetCoordinates[robotMasters[i]];
            else
            {
                coordinates[i] = new Coords(availableCoordinates[coordinatesIx], availableCoordinates[coordinatesIx + 1]);
                coordinatesIx += 2;
            }
        }

        // These must all have at least 3 because each of them can potentially occur 3 times
        var allAABatteries = BombInfo.GetBatteryCount(Battery.AA) + BombInfo.GetBatteryCount(Battery.AAx3) + BombInfo.GetBatteryCount(Battery.AAx4);
        var brules = rnd.ShuffleFisherYates(newArray(
            new EdgeworkRule("# of batteries", BombInfo.GetBatteryCount()),
            new EdgeworkRule("# of D batteries", BombInfo.GetBatteryCount(Battery.D)),
            new EdgeworkRule("# of AA batteries", allAABatteries),
            new EdgeworkRule("# of battery holders", BombInfo.GetBatteryHolderCount())).ToList());

        var irules = rnd.ShuffleFisherYates(newArray(
            new EdgeworkRule("# of indicators", BombInfo.GetIndicators().Count()),
            new EdgeworkRule("# of lit indicators", BombInfo.GetOnIndicators().Count()),
            new EdgeworkRule("# of unlit indicators", BombInfo.GetOffIndicators().Count())).ToList());

        var prules = rnd.ShuffleFisherYates(newArray(
            new EdgeworkRule("# of ports", BombInfo.GetPortCount()),
            new EdgeworkRule("# of port types", BombInfo.CountUniquePorts()),
            new EdgeworkRule("# of port plates", BombInfo.GetPortPlates().Count())).ToList());

        var srules = rnd.ShuffleFisherYates(newArray(
            new EdgeworkRule("first SN digit", BombInfo.GetSerialNumberNumbers().First()),
            new EdgeworkRule("second SN digit", BombInfo.GetSerialNumberNumbers().Skip(1).First()),
            new EdgeworkRule("last SN digit", BombInfo.GetSerialNumberNumbers().Last())).ToList());

        var inds = rnd.ShuffleFisherYates(new List<string> { "SND", "CLR", "CAR", "IND", "FRQ", "SIG", "NSA", "MSA", "TRN", "BOB", "FRK" });
        var ports = rnd.ShuffleFisherYates(new List<Port> { Port.DVI, Port.Parallel, Port.PS2, Port.RJ45, Port.Serial, Port.StereoRCA });
        var operatorNames = new[] { "=", "≤", "≥", "≠" };

        var ops = new Func<int, int, int, bool>((x, y, op) =>
        {
            switch (op)
            {
                case 0: return x == y;
                case 1: return x <= y;
                case 2: return x >= y;
                default: return x != y;
            }
        });

        var possibleAliveConditions = newArray<Func<int, bool, Rule>>(
            (op, presence) => { var indicator = inds[0]; inds.RemoveAt(0); return new Rule(string.Format("indicator labeled “{0}” {1}", indicator, presence ? "present" : "absent"), _ => !presence ^ BombInfo.IsIndicatorPresent(indicator)); },
            (op, presence) => { var indicator = inds[0]; inds.RemoveAt(0); return new Rule(string.Format("lit indicator labeled “{0}” {1}", indicator, presence ? "present" : "absent"), _ => !presence ^ BombInfo.IsIndicatorOn(indicator)); },
            (op, presence) => { var indicator = inds[0]; inds.RemoveAt(0); return new Rule(string.Format("unlit indicator labeled “{0}” {1}", indicator, presence ? "present" : "absent"), _ => !presence ^ BombInfo.IsIndicatorOff(indicator)); },
            (op, presence) => { var number = rnd.Next(1, 6); return new Rule(string.Format("E-Tanks spot {0} A{1}", operatorNames[op], number), eTankNumber => ops(eTankNumber, number, op), eTanks: true); },
            (op, presence) => { var brule = brules[0]; brules.RemoveAt(0); var number = rnd.Next(1, 11); return new Rule(string.Format("{0} {1} {2}", brule.Name, operatorNames[op], number), _ => ops(brule.Value, number, op)); },
            (op, presence) => { var irule = irules[0]; irules.RemoveAt(0); var number = rnd.Next(2, 6); return new Rule(string.Format("{0} {1} {2}", irule.Name, operatorNames[op], number), _ => ops(irule.Value, number, op)); },
            (op, presence) => { var prule = prules[0]; prules.RemoveAt(0); var number = rnd.Next(2, 8); return new Rule(string.Format("{0} {1} {2}", prule.Name, operatorNames[op], number), _ => ops(prule.Value, number, op)); },
            (op, presence) => { var srule = srules[0]; srules.RemoveAt(0); var number = rnd.Next(0, 10); return new Rule(string.Format("{0} {1} {2}", srule.Name, operatorNames[op], number), _ => ops(srule.Value, number, op)); },
            (op, presence) => { var port = ports[0]; ports.RemoveAt(0); return new Rule(string.Format("{0} port {1}", port, presence ? "present" : "absent"), _ => !presence ^ BombInfo.IsPortPresent(port)); },
            (op, presence) => { var brule = brules[0]; brules.RemoveAt(0); var irule = irules[0]; irules.RemoveAt(0); return new Rule(string.Format("{0} {1} {2}", brule.Name, operatorNames[op], irule.Name), _ => ops(brule.Value, irule.Value, op)); },
            (op, presence) => { var brule = brules[0]; brules.RemoveAt(0); var prule = prules[0]; prules.RemoveAt(0); return new Rule(string.Format("{0} {1} {2}", brule.Name, operatorNames[op], prule.Name), _ => ops(brule.Value, prule.Value, op)); },
            (op, presence) => { var brule = brules[0]; brules.RemoveAt(0); var srule = srules[0]; srules.RemoveAt(0); return new Rule(string.Format("{0} {1} {2}", brule.Name, operatorNames[op], srule.Name), _ => ops(brule.Value, srule.Value, op)); },
            (op, presence) => { var irule = irules[0]; irules.RemoveAt(0); var prule = prules[0]; prules.RemoveAt(0); return new Rule(string.Format("{0} {1} {2}", irule.Name, operatorNames[op], prule.Name), _ => ops(irule.Value, prule.Value, op)); },
            (op, presence) => { var irule = irules[0]; irules.RemoveAt(0); var srule = srules[0]; srules.RemoveAt(0); return new Rule(string.Format("{0} {1} {2}", irule.Name, operatorNames[op], srule.Name), _ => ops(irule.Value, srule.Value, op)); },
            (op, presence) => { var prule = prules[0]; prules.RemoveAt(0); var srule = srules[0]; srules.RemoveAt(0); return new Rule(string.Format("{0} {1} {2}", prule.Name, operatorNames[op], srule.Name), _ => ops(prule.Value, srule.Value, op)); }
        ).ToList();

        if (rnd.Seed != 1)
        {
            possibleAliveConditions.Add((op, presence) => { var number = rnd.Next(5, 31); return new Rule(string.Format("# of modules on the bomb {0} {1}", operatorNames[op], number), _ => ops(BombInfo.GetModuleNames().Count, number, op)); });
            possibleAliveConditions.Add((op, presence) => { var number = rnd.Next(10, 61); return new Rule(string.Format("Starting time of the bomb {0} {1} minutes", operatorNames[op], number), _ => ops(time, number, op)); });
        }
        rnd.ShuffleFisherYates(possibleAliveConditions);

        var aliveConditions = new Rule[8];
        var anyETanksConditions = false;
        for (var i = 0; i < 8; i++)
        {
            var ruleInfo = possibleAliveConditions[i];
            var op = rnd.Next(0, 4);
            var presence = rnd.Next(0, 2) == 0;
            aliveConditions[i] = ruleInfo(op, presence);
            if (aliveConditions[i].ETanks)
                anyETanksConditions = true;
        }

        var numbers = rnd.ShuffleFisherYates(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        var numbersObjArr = numbers.Cast<object>().ToArray();
        var possibleETanksRules = (rnd.Seed == 1
            ? new[] { new EdgeworkRule("sum of all digits in the serial number", BombInfo.GetSerialNumberNumbers().Sum()) }
            : newArray(
                new EdgeworkRule("sum of all digits in the serial number", BombInfo.GetSerialNumberNumbers().Sum()),
                new EdgeworkRule("sum of the alphabetic positions of the letters in the serial number", BombInfo.GetSerialNumberLetters().Select(ltr => ltr - 'A' + 1).Sum()),
                new EdgeworkRule("sum of the alphabetic positions of the letters in all indicators", BombInfo.GetIndicators().SelectMany(i => i).Select(ltr => ltr - 'A' + 1).Sum()),
                new EdgeworkRule(string.Format("sum of the values of the indicators, where lit={0} and unlit={1}", numbersObjArr), BombInfo.GetOnIndicators().Count() * numbers[0] + BombInfo.GetOffIndicators().Count() * numbers[1]),
                new EdgeworkRule(string.Format("sum of the values of the ports, where parallel={0}, serial={1}, DVI-D={2}, Stereo RCA={3}, PS/2={4} and RJ-45={5} (other port types are 0)", numbersObjArr),
                    BombInfo.GetPortCount(Port.Parallel) * numbers[0] +
                    BombInfo.GetPortCount(Port.Serial) * numbers[1] +
                    BombInfo.GetPortCount(Port.DVI) * numbers[2] +
                    BombInfo.GetPortCount(Port.StereoRCA) * numbers[3] +
                    BombInfo.GetPortCount(Port.PS2) * numbers[4] +
                    BombInfo.GetPortCount(Port.RJ45) * numbers[5]),
                new EdgeworkRule(string.Format("sum of the values of the batteries, where AA={0} and D={1}", numbersObjArr), allAABatteries * numbers[0] + BombInfo.GetBatteryCount(Battery.D) * numbers[1]),
                new EdgeworkRule("number of modules on the bomb (including needies)", BombInfo.GetModuleNames().Count),
                new EdgeworkRule("number of modules on the bomb (excluding needies)", BombInfo.GetSolvableModuleNames().Count),
                new EdgeworkRule("number of the current month when the bomb was activated", month),
                new EdgeworkRule("day of the month when the bomb was activated", day),
                new EdgeworkRule("total number of Mega Man 2 modules on the bomb", BombInfo.GetSolvableModuleNames().Count(m => m == "Mega Man 2")))).ToList();
        if (rnd.Seed != 1 && !anyETanksConditions)
        {
            var aliveTmp = aliveConditions.Count(rule => rule.Evaluate(0));
            possibleETanksRules.Add(new EdgeworkRule("total number of robot masters that are alive", aliveTmp));
            possibleETanksRules.Add(new EdgeworkRule("total number of robot masters that are dead", 8 - aliveTmp));
        }

        var eTankRuleIx = rnd.Next(0, possibleETanksRules.Count);
        var eTank = (possibleETanksRules[eTankRuleIx].Value + 4) % 5 + 1;

        // ** END RULE SEED GENERATION ** //


        var availableRobotMasterIxs = Enumerable.Range(0, 8).ToList();

        int ix = Random.Range(0, availableRobotMasterIxs.Count);
        selectedMaster = availableRobotMasterIxs[ix];
        availableRobotMasterIxs.RemoveAt(ix);
        selectedWeapon = availableRobotMasterIxs[Random.Range(0, availableRobotMasterIxs.Count)];

        RobotMastersDisplay.material.mainTexture = RobotMasters.First(tx => tx.name == robotMasters[selectedMaster]);
        WeaponsDisplay.material.mainTexture = Weapons.First(tx => tx.name == robotMasters[selectedWeapon]);

        solution = new string[9];
        solution[8] = "A" + eTank;
        Debug.LogFormat(@"[Mega Man 2 #{0}] ETanks spot is: {1}", moduleId, solution[8]);

        for (int i = 0; i < 8; i++)
        {
            var alive = i == selectedMaster ? true : i == selectedWeapon ? false : aliveConditions[i].Evaluate(eTank);
            Debug.LogFormat(@"[Mega Man 2 #{0}] {1} is {2} ({3})", moduleId, robotMasters[i], alive ? "alive" : "dead", i == selectedMaster ? "robot master shown on module" : i == selectedWeapon ? "weapon shown on module" : aliveConditions[i].Text + " = " + (aliveConditions[i].Evaluate(eTank) ? "true" : "false"));
            solution[i] = alive ? coordinates[i].AliveCoord : coordinates[i].DeadCoord;
        }

        Debug.LogFormat(@"[Mega Man 2 #{0}] Password is: {1}", moduleId, solution.Join(", "));
    }

    private string TwitchHelpMessage = "!{0} press a1 a2 b1 [press button a1, a2 and b1]";

    IEnumerator ProcessTwitchCommand(string command)
    {
        var m = Regex.Match(command, @"^\s*(?:press +)?((?:[a-e][1-5] *)+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!m.Success)
            yield break;

        var buttonsToPress = new List<KMSelectable>();

        foreach (var btn in m.Groups[1].Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (btn.Length != 2)
                yield break;
            var row = char.ToLowerInvariant(btn[0]) - 'a';
            var col = btn[1] - '1';
            if (row < 0 || row >= 5 || col < 0 || col >= 5)
                yield break;
            buttonsToPress.Add(GridPoints[col + 5 * row]);
        }

        yield return null;
        yield return buttonsToPress;
    }
}
