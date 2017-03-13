using UnityEngine;
using System.Collections;

public class Content1 : Content {

    private char[] nums = { '1', '2', '3', '4' };

    public Content1()
    {
        name = "History";
        description = "Answer questions to check your history";
    }

    public override char getItem()
    {
        return nums[Random.Range(1, nums.Length)];
    }

    public override string getTerm()
    {
        return "History Question?";
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
        int correct = 3;
        Debug.Log("Compare Hook Input: " + hook.input);
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
