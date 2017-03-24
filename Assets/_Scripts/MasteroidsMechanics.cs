using UnityEngine;
using System.Collections;

public class MasteroidsMechanics : Mechanics {

        public void sendHook(char c ,char[] cs)
        {
            //Debug.Log("Send Hook" + c);
            passHook(new CompareHook(c, cs));
        }
}

