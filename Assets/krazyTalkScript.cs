using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class krazyTalkScript : MonoBehaviour
{

    public KMBombModule Module;
    public KMAudio Audio;
    public KMBombInfo Info;
    public KMSelectable[] btns;
    public TextMesh text;
    public MeshRenderer[] btnColors;
    public Material lit, held, blank, done;
    public KMRuleSeedable ruleseed;

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private bool solved;

    private int[] holdTimes = { 0, 0, 0, 0 };
    private int[] releaseTimes = { 0, 0, 0, 0 };

    int pressedBtns = 0;

    private string[,] holdMessages =
    {
        { "", "", "", "", "", "" },
        { "", "", "", "", "", "" },
        { "", "", "", "", "", "" },
        { "", "", "", "", "", "" }
    };

    private string[,] releaseMessages =
    {
        { "", "", "", "", "", "" },
        { "", "", "", "", "", "" },
        { "", "", "", "", "", ""},
        { "", "", "", "", "", "" }
    };

    private readonly string[] phrases =
    {
        "Quote.", "End quote.", "It says.", "Wait, never mind.", "Never mind.", "Wait, I think this is\nRegular Crazy\nTalk.", "Wait, I think this is\nCrazy Talk.", "Wait, I think this is\nKrazy Talk.",
        "It's too fast, I can't\nread this.", "It's too fast, I can't\nread this!", "Challenge; 3, 2, 1.", "What'd you say?", "What did you say?", "Crazy Talk, with\na C.", "Krazy Talk, with\na C.",
        "Krazy Talk, with\na K.", "Crazy Talk, with\na K.", "Crazy Talc, with\na C.", "Crazy Talc, with\na K.", "She shells sea\nsells by the sea\nshore.", "Sea sells she shells\non the sea shore.",
        "Imagine a managed\nimaginary manager\nimagining a\nmanager.", "It's one from Crazy\nTalk.", "It's one from\nRegular Crazy Talk.", "It literally just\ndisplays nothing.",
        "Exactly what it sas.", "Oh my god,\nsomeone's in\nmy house.", "Oh my god,\nmy parents are\nhere.", "Oh my god,\nthe FBI are here.", "Oh my god,\nnobody's here.", "Oh my god,\nsomebody's in my\nhouse.",
        "Oh my god,\nmy house is gone.", "← → → ← → →", "This one is just\narrow symbols, no\nwords.", "We got a strike.", "I got a strike.", "That was wrong.", "That wasn't right."
    };
    private readonly string[] crazyTalkPhrases =
    {
        "← ← → ← → →", "1 3 2 4", "BLANK", "LITERALLY BLANK",
        "..", "PERIOD PERIOD", "STOP.", "WE JUST BLEW UP", "THE WORD LEFT", "LEFT",
        "IT'S SHOWING\nNOTHING", "THE FOLLOWING\nSENTENCE THE\nWORD NOTHING", "HOLD ON IT'S\nBLANK", "NO, LITERALLY\nNOTHING", "LITERALLY\nNOTHING",
        "THIS ONE IS ALL\nARROW SYMBOLS\nNO WORDS", "FULLSTOP FULLSTOP", "HOLD ON CRAZY\nTALK WHILE I DO\nTHIS NEEDY", "THERE'S NOTHING", "NOTHING"
    };
    private readonly string[] regularCrazyTalkPhrases =
    {
        "We just blew up.", "We ran out of time.", "Repeat?", "Please repeat.", "No christmas\ncrackers.", "Don't wash tennis\nballs.", "How the heck\nam I supposed to\npronounce this?",
        "Were you saying\nsomething?", "Did you say\nsomething?", "Mind the gap.", "Honk honk.", "You have violated\nan area protected\nby a security\nsystem.",
        "Welcome to\nCoffeebucks, may\nI take your name\nplease?", "I'm gonna kill\nthis bomb.", "Wait, hold on, let’s\ndo another module\nfirst.", "The game crashed.", "The game just\ncrashed.",
        "It displays nothing.", "It displays nothing\nat all.", "It displays literally\nnothing.", "Literally nothing.", "It's blank.", "Exactly what it says.", "Exectly what it says.",
        "That's what it says.", "That's what the\nmodule says.", "The buttons don't\ndo anything.", "We solved the\nbomb.", "Contact.", "Challenge 3 2 1.", "She sells sea shells\non the sea shore.",
        "She sells sea shells\nby the sea shore.", "Sea shells she sells\non the sea shore.", "Sea shells she sells\nby the sea shore.", "It’s the one with\nthe sea shells.",
        "It’s the one with\nthe menagerie\nmanager.", "Do you wanna\nplay Fortnite?", ""
    };
    private readonly string[] starts =
        { "“ ", "Quote:", "Quote...", "' ", "It says,", "It says:", "It says comma", "Wait, never mind,", "Never mind,", "Start:", "Start.", "", "", "", "", "", "", "", "", "", "" };
    private readonly string[] ends =
        { " ”", "End quote.", "End quote.", " '", "", "", "", "", "", "Stop.", "Stop.", "I think. It went by\npretty fast.", "I think.", "Holy crap that was\nfast.",
        "Holy crap that went\nby fast.", "Fullstop.", "Stop.", "But spelled wrong.", "But spelled rong.", "In single quotes.", "In double quotes." };

    private int[] phraseValues = { 0, 1, 4, 1, 8, 9, 6, 0, 0, 9, 3, 5, 6, 7, 7, 2, 8, 8, 2, 4, 4, 6, 3, 7, 4, 2, 3, 9, 5, 2, 7, 3, 1, 9, 6, 0, 5, 5, 1, 8 };
    private int[] surroundingValues = { 4, 3, 6, 1, 7, 2, 5, 9, 8, 0, 3, 7, 5, 2, 8, 6, 1, 9, 0, 4, 0 };

    private int shownScreen = 0;
    private bool[] finishedScreens = { false, false, false, false };
    private int[] shownMsg = { 0, 0, 0, 0 };
    private bool[] heldBtns = { false, false, false, false };
    private bool _isHolding;

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        Module.OnActivate += SetUpButtons;
        GenerateModule();
    }

    void SetUpButtons()
    {
        for (int i = 0; i < 4; i++)
        {
            int j = i;

            btns[i].OnInteract += delegate ()
            {
                if (_isHolding)
                    return false;
                _isHolding = true;
                BtnHeld(j);
                btns[j].AddInteractionPunch();
                return false;
            };
        }

        for (int i = 0; i < 4; i++)
        {
            int j = i;

            btns[i].OnInteractEnded += delegate ()
            {
                if (!_isHolding)
                    return;
                _isHolding = false;
                BtnReleased(j);
            };
        }

        for (int i = 0; i < 4; i++)
        {
            btnColors[i].material = blank;
        }
    }

    void GenerateModule()
    {
        for (int i = 0; i < 4; i++)
        {
            holdTimes[i] = Random.Range(0, 10);
            releaseTimes[i] = Random.Range(0, 10);

            for (int x = 0; x < 6; x++)
            {
                int phraseNum = Random.Range(0, phrases.Length + 2);

                if (phraseNum == phrases.Length)
                {
                    holdMessages[i, x] = crazyTalkPhrases[Random.Range(0, crazyTalkPhrases.Length)];
                }

                else if (phraseNum == phrases.Length + 1)
                {
                    holdMessages[i, x] = regularCrazyTalkPhrases[Random.Range(0, regularCrazyTalkPhrases.Length)];
                }

                else
                {
                    holdMessages[i, x] = phrases[phraseNum];
                }

                if (phraseValues[phraseNum] != holdTimes[i])
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        int rndStart = Random.Range(0, starts.Length);

                        if (starts[rndStart] == "" || starts[rndStart] == "' " || starts[rndStart] == "“ ")
                        {
                            holdMessages[i, x] = holdMessages[i, x].Insert(0, starts[rndStart]);
                        }

                        else
                        {
                            holdMessages[i, x] = holdMessages[i, x].Insert(0, starts[rndStart] + "\n");
                        }

                        if (ends[rndStart] == "" || ends[rndStart] == " '" || ends[rndStart] == " ”")
                        {
                            holdMessages[i, x] = holdMessages[i, x] + ends[rndStart];
                        }

                        else
                        {
                            holdMessages[i, x] = holdMessages[i, x] + "\n" + ends[rndStart];
                        }

                        int rndNumber = Random.Range(0, starts.Length);

                        while ((phraseValues[phraseNum] + surroundingValues[rndStart] + surroundingValues[rndNumber]) % 10 != holdTimes[i])
                        {
                            rndNumber = (rndNumber + 1) % starts.Length;
                        }

                        if (starts[rndNumber] == "" || starts[rndNumber] == "'" || starts[rndNumber] == "“")
                        {
                            holdMessages[i, x] = holdMessages[i, x].Insert(0, starts[rndNumber]);
                        }

                        else
                        {
                            holdMessages[i, x] = holdMessages[i, x].Insert(0, starts[rndNumber] + "\n");
                        }

                        if (ends[rndNumber] == "" || ends[rndNumber] == "'" || ends[rndNumber] == "”")
                        {
                            holdMessages[i, x] = holdMessages[i, x] + ends[rndNumber];
                        }

                        else
                        {
                            holdMessages[i, x] = holdMessages[i, x] + "\n" + ends[rndNumber];
                        }
                    }

                    else
                    {
                        int rndNumber = Random.Range(0, starts.Length);

                        while ((phraseValues[phraseNum] + surroundingValues[rndNumber]) % 10 != holdTimes[i])
                        {
                            rndNumber = (rndNumber + 1) % starts.Length;
                        }

                        if (starts[rndNumber] == "" || starts[rndNumber] == "'" || starts[rndNumber] == "“")
                        {
                            holdMessages[i, x] = holdMessages[i, x].Insert(0, starts[rndNumber]);
                        }

                        else
                        {
                            holdMessages[i, x] = holdMessages[i, x].Insert(0, starts[rndNumber] + "\n");
                        }

                        if (ends[rndNumber] == "" || ends[rndNumber] == "'" || ends[rndNumber] == "”")
                        {
                            holdMessages[i, x] = holdMessages[i, x] + ends[rndNumber];
                        }

                        else
                        {
                            holdMessages[i, x] = holdMessages[i, x] + "\n" + ends[rndNumber];
                        }
                    }
                }
            }
        } // Calculate hold messages

        for (int i = 0; i < 4; i++) // Calculate release messages
        {
            int arrayPos = 0;

            for (int x = 0; x < phraseValues.Length; x++)
            {
                if (phraseValues[x] == releaseTimes[i])
                {
                    if (x < phrases.Length)
                    {
                        releaseMessages[i, arrayPos] = phrases[x];
                    }

                    else if (x == phrases.Length)
                    {
                        releaseMessages[i, arrayPos] = crazyTalkPhrases[Random.Range(0, crazyTalkPhrases.Length)];
                    }

                    else
                    {
                        releaseMessages[i, arrayPos] = regularCrazyTalkPhrases[Random.Range(0, regularCrazyTalkPhrases.Length)];
                    }

                    arrayPos++;
                }
            }
        }

        StartCoroutine("Cycle");

        for (int i = 0; i < 4; i++)
        {
            for (int x = 0; x < 6; x++)
            {
                DebugMsg("One message for Screen #" + (i + 1) + " is " + holdMessages[i, x].Replace("\n", " "));
            }
        }
    }

    void BtnHeld(int btnNum)
    {
        if (!solved && !finishedScreens[btnNum])
        {
            var time = (int)Info.GetTime() % 10;

            StopCoroutine("Cycle");

            DebugMsg("You pressed button #" + (btnNum + 1) + " when the last digit of the timer was " + time + ".");
            DebugMsg("You were supposed to hold it when the last digit of the timer was " + holdTimes[btnNum] + ".");

            for (int i = 0; i < 4; i++)
            {
                btnColors[i].material = blank;
            }

            btnColors[btnNum].material = held;

            if (time == holdTimes[btnNum])
            {
                DebugMsg("That was right.");
                heldBtns[btnNum] = true;

                StartCoroutine("FastCycle", btnNum);
            }

            else
            {
                Module.HandleStrike();
                btnColors[btnNum].material = blank;

                DebugMsg("That was wrong. STRIKE!");
                heldBtns[btnNum] = false;

                StartCoroutine("Cycle");
            }
        }
    }

    void BtnReleased(int btnNum)
    {
        if (heldBtns[btnNum] == true && !solved)
        {
            var time = (int)Info.GetTime() % 10;

            DebugMsg("You released button #" + (btnNum + 1) + " when the last digit of the timer was " + time + ".");
            DebugMsg("You were supposed to release it when the last digit of the timer was " + releaseTimes[btnNum] + ".");

            StartCoroutine("Cycle");
            StopCoroutine("FastCycle");

            heldBtns[btnNum] = false;

            if (time == releaseTimes[btnNum])
            {
                DebugMsg("That was right.");
                pressedBtns++;

                finishedScreens[btnNum] = true;
                btnColors[btnNum].material = done;

                if (pressedBtns == 3)
                {
                    Module.HandlePass();
                    DebugMsg("Module solved!");
                    solved = true;
                    StopCoroutine("Cycle");

                    text.text = "Never mind, it\nsolved itself.";

                    for (int i = 0; i < 4; i++)
                    {
                        btnColors[i].material = lit;
                    }
                }
            }

            else
            {
                Module.HandleStrike();
                btnColors[btnNum].material = blank;

                DebugMsg("That was wrong. STRIKE!");
            }
        }
    }

    void DebugMsg(string msg)
    {
        Debug.LogFormat("[Krazy Talk #{0}] {1}", _moduleId, msg);
    }

    IEnumerator Cycle()
    {
        text.text = "";

        while (finishedScreens[shownScreen])
        {
            shownScreen = (shownScreen + 1) % 4;
        }

        yield return new WaitForSeconds(.5f);

        while (!solved)
        {
            while (finishedScreens[shownScreen])
            {
                shownScreen = (shownScreen + 1) % 4;
            }

            btnColors[shownScreen].material = lit;
            text.text = holdMessages[shownScreen, shownMsg[shownScreen]];

            yield return new WaitForSeconds(4.5f);

            btnColors[shownScreen].material = blank;
            shownMsg[shownScreen] = (shownMsg[shownScreen] + 1) % 6;
            shownScreen = (shownScreen + 1) % 4;
            text.text = "";

            yield return new WaitForSeconds(.5f);
        }
    }

    IEnumerator FastCycle(int btnNum)
    {
        text.text = "";
        int shownMsg = 0;
        yield return new WaitForSeconds(.2f);

        while (!solved)
        {
            text.text = releaseMessages[btnNum, shownMsg];
            shownMsg = (shownMsg + 1) % 4;

            yield return new WaitForSeconds(.2f);
        }
    }

    void SetUpRuleseed()
    {
        var rnd = ruleseed.GetRNG();

        rnd.ShuffleFisherYates(phraseValues);
        rnd.ShuffleFisherYates(surroundingValues);
    }

    public string TwitchHelpMessage = "Use !{0} hold 1 on 0 or !{0} release 1 on 0 to hold/release a certain button on a number. Buttons are numbered 1-4 in reading order.";
    IEnumerator ProcessTwitchCommand(string cmd)
    {
        if ((cmd.ToLowerInvariant().StartsWith("hold ") && cmd.Length == 11) || (cmd.ToLowerInvariant().StartsWith("release ") && cmd.Length == 14))
        {
            string btnString, time;
            if (cmd.ToLowerInvariant().StartsWith("hold "))
            {
                btnString = cmd[5].ToString();
                time = cmd[10].ToString();
            }

            else
            {
                btnString = cmd[8].ToString();
                time = cmd[13].ToString();
            }

            int btnNum;
            string[] numbers = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            if (btnString == "1")
                btnNum = 0;

            else if (btnString == "2")
                btnNum = 1;

            else if (btnString == "3")
                btnNum = 2;

            else if (btnString == "4")
                btnNum = 3;

            else
            {
                yield return "sendtochaterror That's not a button I can press.";
                yield break;
            }

            if (!numbers.Contains(time))
            {
                yield return "sendtochaterror That's not a time I can use.";
                yield break;
            }

            if (cmd.ToLowerInvariant().StartsWith("hold ") && heldBtns.Contains(true))
            {
                yield return "sendtochaterror A button is already held.";
                yield break;
            }

            else if (cmd.ToLowerInvariant().StartsWith("release ") && heldBtns[btnNum] == false)
            {
                yield return "sendtochaterror That button hasn't been held yet.";
                yield break;
            }

            yield return null;
            while (!((int)Info.GetTime()).ToString().EndsWith(time))
            {
                yield return "trycancel";
            }

            if (cmd.ToLowerInvariant().StartsWith("hold "))
                btns[btnNum].OnInteract();
            else
                btns[btnNum].OnInteractEnded();

            yield break;
        }

        else
            yield break;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        int heldBtn = -1;
        while (!solved)
        {
            nextStage:
            yield return null;
            if (!heldBtns.Contains(true))
            {
                notYetHeld:
                for (int i = 0; i < 4; i++)
                {
                    if (!finishedScreens[i] && (int)Info.GetTime() % 10 == holdTimes[i])
                    {
                        btns[i].OnInteract();
                        heldBtn = i;
                        goto nowHolding;
                    }
                }
                yield return true;
                goto notYetHeld;
            }
            nowHolding:
            yield return null;
            while ((int)Info.GetTime() % 10 != releaseTimes[heldBtn])
            {
                yield return true;
            }
            btns[heldBtn].OnInteractEnded();
            if (!solved)
            {
                heldBtn = -1;
                goto nextStage;
            }
        }
    }
}