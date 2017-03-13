using UnityEngine;
using System;
using System.Runtime.InteropServices;

/*
 * Much of the logic in here is based off of:
 * Craig, S. D., D'Mello, S., Witherspoon, A., & Graesser, A. (2008).
 * Emote aloud during learning with AutoTutor: Applying the Facial Action Coding System to cognitive–affective states during learning.
 * Cognition and Emotion, 22(5), 777-788.
 */

// Without the Tracker script from Visage, 99% of this won't work.
[RequireComponent(typeof(Tracker))]

/// <summary>
/// This class is the wrapper that encases the entirety of the emotion
/// based components of the game. If you want to modify anything in
/// regards to how emotion is tracked, handled, etc. this is the place
/// to do it. Keep in mind, our results here can only be as good as what
/// we get from Visage. If you need to modify the way the camera works,
/// try the Tracker class from Visage. If you want to change the functionality
/// that this project can use from the Visage stuff, try VisageUnityPlugin.cpp
/// in the Visage SDK. For deeper changes, you will need to dig further
///  into the Visage SDK.
/// </summary>
public class EmotionWrapper : MonoBehaviour
{
    #region Settings
    
    
    // This bool is the master switch. If you uncheck this in the editor, no emotion or video stuff happens.
    public bool tracking;
    public bool showUI; // Switches whether the data is shown to the player or not.
    public GameObject video; // Set this in the editor as the Video Pane prefab, otherwise facecam will not work!

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    // THESE ARE THE VARIABLES YOU ARE MOST LIKELY INTERESTED IN CHANGING!!

    private const int ANALYSISDELAY = 150; // Number of frames to delay after detecting a state, before we try to get a new one.
    private const int EMOTIONMAX = 60; // Number of frames until affective state is confirmed. Needs to be short (Craig et al., 2008)!
    private const float THRESHOLD = 0.25f; // The minimum before an emotion is considered significant.
    private const int NUMFRAMES = 25; // The number of frames of states we will retain as our average for determining current state.

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    // These variables are related to difficulty. You may be interested in modifying these.

    private int[,] swarmNums = new int[5, 5]; // Holds the number of each of the five types of ships for each of the five difficulty levels. See function initDifficulty.
    private Difficulty curDifficulty = Difficulty.Three; // The default difficulty level.

    ///////////////////////////////////////////////////////////////////////////////////////////////////////

    // The rest of these are misc. variables that you probably do not want to change.

    private bool haveRecentData = false;
    private float[] probs = new float[6];
    private int currentDelay = 0;
    private int currentEmotionDuration = 0;
    private AffectiveStates currentState = AffectiveStates.None;
    private AffectiveStates lastState = AffectiveStates.None;
    private AffectiveStates[] lastFrames = new AffectiveStates[NUMFRAMES];
    private bool wasWaiting = false;
    private int frame = 0;
    private bool waitForWave = false;

    private const int ANGER = 0;
    private const int DISGUST = 1;
    private const int FEAR = 2;
    private const int HAPPINESS = 3;
    private const int SADDNESS = 4;
    private const int SURPRISE = 5;

    #endregion

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < lastFrames.Length; i++)
        {
            lastFrames[i] = AffectiveStates.None;
        }
        initDifficulty();
        FileManagement.init();
        DontDestroyOnLoad(this);
    }

    // Release camera on exit. Make sure you do this, or else Unity usually crashes when you start the game again!
    void OnApplicationQuit()
    {
        VisageTrackerNative._freeCamera();
    }

    // Don't try to change difficulty (causes NullPointer) or bring up facecam on main menu.
    void OnLevelWasLoaded(int level)
    {
        //FileManagement.setTime();
        if (level != 0)
        {
            //*****FileManagement.startLevel(level);
            setDifficulty(0);
            currentDelay = ANALYSISDELAY;
            if ((video != null) && tracking)
            {
                Instantiate(video);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (tracking)
        {
            if (gameObject.GetComponent<Tracker>().TrackerStatus != 0)
            {

                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                // Emotions are acqured here! Be extremely careful about making any changes to these two lines!
                // The entire emotion system depends on these working successfully.
                IntPtr nums = VisageTrackerNative._getEmotions();
                // Due to cross language type mangling, we have to marshal the result into something we can use.
                Marshal.Copy(nums, probs, 0, 6);
                ///////////////////////////////////////////////////////////////////////////////////////////////////////
                FileManagement.dump(probs);
                if (probs[0] != -9999) // -9999 is the "error" code from the plugin.
                {
                    for (int i = 0; i < probs.Length; i++)
                    {
                        // Truncate to three places for easier display
                        probs[i] = (float)Math.Truncate(1000 * probs[i]) / 1000;
                    }
                    haveRecentData = true;
                }
                else
                {
                    haveRecentData = false;
                    currentState = AffectiveStates.None;
                }
            }
            else
            {
                haveRecentData = false;
            }
            // If we have gotten recent data, and are not waiting for the next wave, analyse it.
            if (haveRecentData && !waitForWave)
            {
                analyzeData();
            }
        }
    }

    // Displays the current emotion info and state on screen.
    void OnGUI()
    {
        /*if (tracking && showUI)
        {
            GUI.Box(GameUIScript.ScreenRect(GUIConstants.GUI_LEFT_COLUMN_X, GUIConstants.GUI_MIDDLE_ROW_Y + 0.2f, GUIConstants.HEALTH_LABEL_WIDTH, 0.156f), "Emotions");
            GUI.Label(GameUIScript.ScreenRect(GUIConstants.GUI_LEFT_COLUMN_X + 0.001f, GUIConstants.GUI_MIDDLE_ROW_Y + .228f, GUIConstants.HEALTH_LABEL_WIDTH, .075f),
                                string.Format("Anger: {0}", probs[ANGER]));
            GUI.Label(GameUIScript.ScreenRect(GUIConstants.GUI_LEFT_COLUMN_X + 0.001f, GUIConstants.GUI_MIDDLE_ROW_Y + .248f, GUIConstants.HEALTH_LABEL_WIDTH, .075f),
                                string.Format("Disgust: {0}", probs[DISGUST]));
            GUI.Label(GameUIScript.ScreenRect(GUIConstants.GUI_LEFT_COLUMN_X + 0.001f, GUIConstants.GUI_MIDDLE_ROW_Y + .268f, GUIConstants.HEALTH_LABEL_WIDTH, .075f),
                                string.Format("Fear: {0}", probs[FEAR]));
            GUI.Label(GameUIScript.ScreenRect(GUIConstants.GUI_LEFT_COLUMN_X + 0.001f, GUIConstants.GUI_MIDDLE_ROW_Y + .288f, GUIConstants.HEALTH_LABEL_WIDTH, .075f),
                                string.Format("Happy: {0}", probs[HAPPINESS]));
            GUI.Label(GameUIScript.ScreenRect(GUIConstants.GUI_LEFT_COLUMN_X + 0.001f, GUIConstants.GUI_MIDDLE_ROW_Y + .308f, GUIConstants.HEALTH_LABEL_WIDTH, .075f),
                                string.Format("Sadness: {0}", probs[SADDNESS]));
            GUI.Label(GameUIScript.ScreenRect(GUIConstants.GUI_LEFT_COLUMN_X + 0.001f, GUIConstants.GUI_MIDDLE_ROW_Y + .328f, GUIConstants.HEALTH_LABEL_WIDTH, .075f),
                                string.Format("Surprise: {0}", probs[SURPRISE]));

            GUI.Box(GameUIScript.ScreenRect(GUIConstants.GUI_LEFT_COLUMN_X, GUIConstants.GUI_MIDDLE_ROW_Y + 0.38f, GUIConstants.HEALTH_LABEL_WIDTH, 0.04f), "Current State");
            string state = "";
            switch (currentState)
            {
                case AffectiveStates.None:
                    state = "None";
                    break;
                case AffectiveStates.Boredom:
                    state = "Boredom";
                    break;
                case AffectiveStates.Frustration:
                    state = "Frustration";
                    break;
                case AffectiveStates.Flow:
                    state = "Flow";
                    break;
            }
            GUI.Label(GameUIScript.ScreenRect(GUIConstants.GUI_LEFT_COLUMN_X + 0.003f, GUIConstants.GUI_MIDDLE_ROW_Y + 0.4f, GUIConstants.HEALTH_LABEL_WIDTH, .075f), state);
        }*/
    }

    /// <summary>
    /// We don't want to keep changing the difficulty every few seconds.
    /// This is because a player will not have a chance to notice a change
    /// until the next wave is sent out. So, we will stall until the next time
    /// a wave spawns. This gets called when the WaveManager sends out a new
    /// wave of ships. Since there will be at least a few seconds between
    /// the time the wave spawns and it comes into view for the player, we
    /// still need to delay a little bit from when we send out the wave.
    /// </summary>
    /*public void waveStart()
    {
        waitForWave = false;
        currentDelay = ANALYSISDELAY * 5;
        FileManagement.waveSpawn();
    }*/

    /// <summary>
    /// Algorithmically sets up each difficulty level to have progressively more ships than the previous one.
    /// Numbers are stored in swarmNums, a 2D [x, y] array where x is a difficulty level and y is the number
    /// of ships of that type. There are five types of ships, so unless more are added, y should be less than
    /// or equal to five.
    /// If you want to modify the difficulty, you will want to modify this algorithm.
    /// </summary>
    private void initDifficulty()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                swarmNums[i, j] = (i + 4) - (j - i);
            }
        }
    }

    // This is where we begin trying to determine the player's affective state.
    private void analyzeData()
    {
        // Are we waiting between changes?
        if (currentDelay > 0)
        {
            currentDelay--;
        }
        else
        {
            // Do we need to update the data?
            if (wasWaiting)
            {
                haveRecentData = false;
                wasWaiting = false;
                // Reset stored frames to ensure we don't accidentally make judgements off them.
                for(int i = 0; i < lastFrames.Length; i++)
                {
                    lastFrames[i] = AffectiveStates.None;
                }
                FileManagement.delayEnd();
            }
            else
            {
                AffectiveStates update = averageState();
                // Are they still in the same state?
                if (currentState == update)
                {
                    // Only increase this if we are tracking an actual emotion
                    if (currentState != AffectiveStates.None)
                    {
                        currentEmotionDuration++;
                    }
                }
                else
                {
                    // State has changed. Reset everything.
                    currentState = update;
                    currentEmotionDuration = 0;
                    FileManagement.stateChange(currentState);
                }

                // Have they sustained that emotion long enough to consider it their state?
                if (currentEmotionDuration == EMOTIONMAX)
                {
                    // Reset everything.
                    lastState = currentState;
                    wasWaiting = true;
                    currentDelay = ANALYSISDELAY;
                    currentEmotionDuration = 0;
                    FileManagement.emotionMax(currentState);
                    waitForWave = true;

                    // Determine how (if at all) we need to modify difficulty.
                    switch (currentState)
                    {
                        case AffectiveStates.Boredom:
                            curDifficulty = stepUp();
                            setDifficulty(1);
                            break;
                        case AffectiveStates.Frustration:
                            curDifficulty = stepDown();
                            setDifficulty(1);
                            break;
                        case AffectiveStates.Flow:
                            // If they're in Flow, don't change anything for awhile.
                            currentDelay = ANALYSISDELAY * 5;
                            break;
                    }
                }
            }
        }
    }

    // This is meant to "smooth" the data and make it less noisy by averaging the last NUMFRAMES.
    private AffectiveStates averageState()
    {
        AffectiveStates state = AffectiveStates.None;
        lastFrames[frame] = determineState(); // This gets the state for the current frame.
        frame = ((frame + 1) % NUMFRAMES);
        int nones = 0;
        int frust = 0;
        int bored = 0;
        int flow = 0;
        // Count the number of each state we've got on record.
        for (int i = 0; i < NUMFRAMES; i++)
        {
            switch (lastFrames[i])
            {
                case AffectiveStates.Boredom:
                    bored++;
                    break;
                case AffectiveStates.Flow:
                    flow++;
                    break;
                case AffectiveStates.Frustration:
                    frust++;
                    break;
                default:
                    nones++;
                    break;
            }
        }
        // Most frequent wins.
        if ((frust > bored) && (frust > flow) && (frust > nones))
        {
            state = AffectiveStates.Frustration;
        }
        else if ((bored > flow) && (bored > nones))
        {
            state = AffectiveStates.Boredom;
        }
        else if (flow > nones)
        {
            state = AffectiveStates.Flow;
        }
        // else - we already have none stored in state.
        return state;
    }

    /// <summary>
    /// This is the hard one, where the real work happens. This is where we determine the affective state from the current frame.
    /// Visage gives us the emotion estimations, that's the easy part. The hard part is going from those six measures to our
    /// affective states. Literature, such as Craig et al. (2008), tell us that the affective states will appear as the following:
    /// Boredom: Neutrality, no real emotion showed.
    /// Frustration: High anger, low happiness. May also include disgust, but disgust proved unreliable.
    /// Flow: Surprise/Awe and low sadness. May also appear as neutrality, which can cause confusion when compared to boredom.
    /// Based on those, the measures used below were created to try and detect each of these. Feel free to modify this as
    /// needed, but be aware that even small changes will have a major impact on affective state detection. If you feel that
    /// the states are being detected incorrectly, this is the part to modify.
    /// </summary>
    private AffectiveStates determineState()
    {
        AffectiveStates state = AffectiveStates.None;
        // DETECT BOREDOM: If all estimations are below THRESHOLD, player is showing neutrality.
        bool nonBored = false;
        for (int i = 0; ((i < probs.Length) && (!nonBored)); i++)
        {
            if (probs[i] > THRESHOLD)
            {
                nonBored = true;
            }
        }
        // If player is not bored, they must be something else.
        if (nonBored)
        {
            // DETECT FRUSTRATION: High anger and low happiness is a sign of frustration.
            if ((probs[ANGER] > THRESHOLD) && (probs[HAPPINESS] < THRESHOLD))
            {
                state = AffectiveStates.Frustration;
            }
            // Player is not frustrated, are they in flow?
            else
            {
                // DETECT FLOW: Noticible surprise and low sadness show flow.
                if ((probs[SURPRISE] > THRESHOLD) && (probs[SADDNESS] < THRESHOLD))
                {
                    state = AffectiveStates.Flow;
                }
                // NOTE! No else here. If we get here, all of our measures have failed.
                // This means that the player is in a state we can't recognize, so we return None.
            }
        }
        else
        {
            // Double check that they might be in Flow, as it can look a lot like boredom.
            // DETECT FLOW: If they appear bored and are currently in flow, or just were in flow, they're probably in flow still/again.
            if ((currentState == AffectiveStates.Flow) || (lastState == AffectiveStates.Flow))
            {
                state = AffectiveStates.Flow;
            }
            // Nope, they are truely bored.
            else
            {
                state = AffectiveStates.Boredom;
            }
        }
        return state;
    }

    // Increases the difficulty level by one, if possible.
    private Difficulty stepUp()
    {
        Difficulty diff = curDifficulty;
        switch (curDifficulty)
        {
            case Difficulty.One:
                diff = Difficulty.Two;
                break;
            case Difficulty.Two:
                diff = Difficulty.Three;
                break;
            case Difficulty.Three:
                diff = Difficulty.Four;
                break;
            case Difficulty.Four:
                diff = Difficulty.Five;
                break;
        }
        return diff;
    }

    // Decreases the difficulty level by one, if possible.
    private Difficulty stepDown()
    {
        Difficulty diff = curDifficulty;
        switch (curDifficulty)
        {
            case Difficulty.Two:
                diff = Difficulty.One;
                break;
            case Difficulty.Three:
                diff = Difficulty.Two;
                break;
            case Difficulty.Four:
                diff = Difficulty.Three;
                break;
            case Difficulty.Five:
                diff = Difficulty.Four;
                break;
        }
        return diff;
    }

    /// <summary>
    /// This is the function where the difficulty is actually changed.
    /// It updates the current wave (if adjustment = 0, which is only
    /// used in OnLevelLoad) or the next wave
    /// (if adjustment = 1) to use the number of ships stored in 
    /// swarmNums[curDifficulty, x], where x is the type of ship.
    /// You probably do not want to modify this, even if you are changing
    /// the difficulty. Instead, you probably want to modify the initDifficulty
    /// function.
    /// </summary>
    /// <param name="adjustment">The number to add to the current wave number to determine which wave to modify.</param>
    private void setDifficulty(int adjustment)
    {
        FileManagement.difficultyChange(curDifficulty);
        GameObject obj = null;
        // Sometimes the manager can't be found when first opening the level
        while (obj == null)
        {
            obj = GameObject.Find("WaveManager");
        }
        //********WaveManager manage = obj.GetComponent<WaveManager>();
        // Check first that there is a next wave we can manipulate.
        /*******if (manage.getWaveNum() < manage.m_Waves.Count)
        {
            int nextDiff = 0;
            // Convert from enum to int to simplify wave updating operation.
            switch (curDifficulty)
            {
                case Difficulty.Two:
                    nextDiff = 1;
                    break;
                case Difficulty.Three:
                    nextDiff = 2;
                    break;
                case Difficulty.Four:
                    nextDiff = 3;
                    break;
                case Difficulty.Five:
                    nextDiff = 4;
                    break;
            }

            /*
             * Let me explain a bit here. WaveManager holds some number of Wave objects.
             * Each of these waves holds a set of numbers for how many of each type of
             * ship it will send out. This is why we need two for loops here. The outter
             * one goes through the list of Wave Objects (manage.m_Waves). The inner one
             * goes through that wave's list of numbers of ships (manage.m_Waves.m_Amount).
             */
             
            /*********for (int wave = adjustment + manage.m_Waves.Count; wave < manage.m_Waves.Count; wave++)
            {
                for (int ship = 0; ship < manage.m_Waves[wave].m_Amount.Count; ship++)
                {
                    manage.m_Waves[wave].m_Amount[ship] = swarmNums[nextDiff, ship];
                }
            }
        }*/
    }
}
