using UnityEngine;
using System.Collections;

public class Content2 : Content {

    private char[] nums = { '1', '2', '3', '4' };

    public Content2()
    {
        name = "Geography";
        description = "Master your geography";
    }

    public override char getItem()
    {
        return nums[Random.Range(1, nums.Length)];
    }

    public override string getTerm()
    {
        return "Geographical Question?";
    }

    protected override void customHook(Hook hook)
    {
        switch (hook.type)
        {
            case HookType.Compare:
                compareHook((CompareHook)hook);
                break;
            default:
                base.customHook(hook);
                break;
        }
    }

    private void compareHook(CompareHook hook)
    {
        int correct = 2;
        if (hook.input == correct)
        {
            lastActionValid = true;
        }
        else
        {
            lastActionValid = false;
        }
    }

}
