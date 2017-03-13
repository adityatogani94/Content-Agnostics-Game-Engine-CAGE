using UnityEngine;
using System;
using System.IO;
using System.Collections;

/// <summary>
/// This class serves as the single interface for all file writing.
/// The idea is to report all important events so that the game's
/// actions can be coded and compared to viewed reactions from video.
/// </summary>
public static class FileManagement
{
    private static bool wasInit = false;
    private static string FILENAME;
    private static string DUMPNAME;

    // Get the date to use as the file name.
    public static void init()
    {
        wasInit = true;
        // To ensure files are not overwritten and are easily identifiable, we will name them wih the current date and time.
        int day = DateTime.Now.Day;
        int month = DateTime.Now.Month;
        int year = DateTime.Now.Year;
        int hour = DateTime.Now.Hour;
        int minute = DateTime.Now.Minute;
        int second = DateTime.Now.Second;
        FILENAME = (GameInfo.gameTitle + " - " + month + "-" + day + "-" + year + "-" + hour + "-" + minute + "-" + second);
        // The dump file holds all the emotion measurements for each frame. Put in a separate file to not clog other data.
        DUMPNAME = FILENAME + "-EMOTION-DUMP.txt";
        FILENAME += ".txt";

        // Test creating files. 
        print("Note: All times listed are in seconds since the start of the game!");
        print("Some times (such as this message) may be the same. This means both happened within the same frame in Unity.");
        print("Note: -9999 means emotions could not be analyzed on that frame. (Likely because the face could not be tracked)");

    }

    // Helper to get timestamp string.
    private static string getTime()
    {
        return "[" + Time.time + "] ";
    }

    // Helper to translate affective state.
    private static string getState(AffectiveStates state)
    {
        string print;
        switch (state)
        {
            case AffectiveStates.Boredom:
                print = "Boredom";
                break;
            case AffectiveStates.Flow:
                print = "Flow";
                break;
            case AffectiveStates.Frustration:
                print = "Frustration";
                break;
            default:
                print = "None";
                break;
        }
        return print;
    }

    // Helper to translate difficulty.
    private static string getDiff(Difficulty diff)
    {
        string print;
        switch (diff)
        {
            case Difficulty.One:
                print = "One";
                break;
            case Difficulty.Two:
                print = "Two";
                break;
            case Difficulty.Three:
                print = "Three";
                break;
            case Difficulty.Four:
                print = "Four";
                break;
            default:
                print = "Five";
                break;
        }
        return print;
    }

    // Helper to open and write to the file. Keeping all the possible errors to one point.
    private static void print(string message)
    {
        if (!wasInit)
        {
            init();
        }

        using (StreamWriter file = new StreamWriter(FILENAME, true))
        {
            // The using command here automatically closes and flushes the file.
            file.WriteLine(getTime() + message);
        }

    }

    // Called when a new affective state is detected.
    public static void stateChange(AffectiveStates state)
    {
        print("STATE CHANGE - NEW STATE -> " + getState(state));
    }

    // Called when the difficulty is changed.
    public static void difficultyChange(Difficulty diff)
    {
        print("DIFFICULTY CHANGE - NEW DIFFICUTLY -> " + getDiff(diff));
    }

    // Called when an emotion has been detected long enough for us to make judgements off of it.
    public static void emotionMax(AffectiveStates state)
    {
        print("EMOTION MAX REACHED - The player has been detected in " + getState(state) + " for long enough to consider it their new state.");
    }

    // Called after a detection delay ends so it is obvious when facial analysis is occurring again.
    public static void delayEnd()
    {
        print("END ANALYSIS DELAY - Now Analyzing Player Emotions Again");
    }

    // Called to report emotions on that frame.
    public static void dump(float[] arr)
    {
        if (!wasInit)
        {
            init();
        }

        using (StreamWriter file = new StreamWriter(DUMPNAME, true))
        {
            file.WriteLine(getTime() + "Anger: " + arr[0]);
            file.WriteLine(getTime() + "Disgust: " + arr[1]);
            file.WriteLine(getTime() + "Fear: " + arr[2]);
            file.WriteLine(getTime() + "Happiness: " + arr[3]);
            file.WriteLine(getTime() + "Sadness: " + arr[4]);
            file.WriteLine(getTime() + "Surprise: " + arr[5]);
        }
    }

    // Public version to accept any sort of message.
    public static void printToFile(string message)
    {
        print(message);
    }
}