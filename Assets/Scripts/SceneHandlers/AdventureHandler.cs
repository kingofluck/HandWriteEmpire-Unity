﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using DragonBones;
using UnityEngine;
using UnityEngine.UI;

public class AdventureHandler : MonoBehaviour
{
    //书写冷却时间
    public double WriteFireTime;
    private double writeFireTime;

    //敌人攻击冷却时间
    public double BossFireTime;
    private double bossFireTime;
    public Text timeText;


    private bool isStartWrite = false;
    private bool isUp = false;

    public ChineseInfo[] infos;
    public int currentChinese = 0;
    public Text chinesePinYin;
    public Text chineseContent;
    public Text chineseCurrentPinYin;

    private string[] pinYinArray;
    private string[] contentArray;
    private int currentCharacter = 0;
    private string regResult;

    public ButtonStatusManager btnManager;

    public UnityArmatureComponent attackRole;

    //是否是新的识别
    private bool isNewRec = false;

    //是否是新的动画
    private bool isNewAnim = false;
    private string newAnimName = "";

    //Boss攻击是否计时
    private bool isCalcTime = true;


    private void Start()
    {
        RequestInfo();

        bossFireTime = BossFireTime;

        writeFireTime = WriteFireTime;

        UpdateChineseInfo(infos[currentChinese]);

        //设置动画监听
        attackRole.AddDBEventListener(EventObject.COMPLETE, OnAnimationEventHandler);
    }

    //请求网络数据
    private void RequestInfo()
    {
        infos = new ChineseInfo[4];
        infos[0] = new ChineseInfo("nǐ hǎo", "你 好", "用于有礼貌的打招呼或表示与人见面时的问候");
        infos[1] = new ChineseInfo("kē jì", "科 技", "社会上习惯于把科学和技术连在一起，统称为“科技”。实际二者既有密切联系，又有重要区别。科学解决理论问题，技术解决实际问题");
        infos[2] = new ChineseInfo("xiàn zài", "现 在", "现世,今生;眼前一刹那");
        infos[3] = new ChineseInfo("wèi lái", "未 来", "从现在往后的时间");
    }

    private void Update()
    {
        if (isStartWrite && isUp)
        {
            writeFireTime -= Time.deltaTime;
            if (writeFireTime <= 0)
            {
                writeFireTime = WriteFireTime;
                isStartWrite = false;
                isUp = false;
                HWRRec();
            }
        }
        else
        {
            writeFireTime = WriteFireTime;
        }

        if (isCalcTime)
        {
            //Boss攻击
            bossFireTime -= Time.deltaTime;
            if (bossFireTime <= 0)
            {
                isCalcTime = false;
                bossFireTime = BossFireTime;
                FadeInRoleAnim(attackRole, "behurt");
            }
        }

        timeText.text = bossFireTime.ToString("Boss : 0.00s");
    }


    public void SetTimerStart(string state)
    {
        if ("Down".Equals(state))
        {
            //当按下的时候表示开始写字
            isStartWrite = true;
        }
        else if ("Move".Equals(state))
        {
            isUp = false;
            writeFireTime = WriteFireTime;
        }
        else if ("Up".Equals(state))
        {
            isUp = true;
        }
    }

    //获取Android手写识别后的结果
    public void CallHWRRec()
    {
        AndroidUtil.Call("hwrRec");
    }

    //手写识别的功能
    public void HWRRec()
    {
        //TODO 限制输入的个数以及实现修改功能
        CallHWRRec();
    }
    //手写模块识别结果回调，在Anroid那边调用
    public void OnGetRecResult(string results)
    {
        AndroidUtil.Log("识别到的结果:" + results);
        if (results != null && results.Length >= 1)
        {
            var resultArr = results.Split(';');
            if (resultArr.Length > 0)
            {
                AddCharacter(resultArr[0]);
                currentCharacter++;
                if (currentCharacter == contentArray.Length)
                    btnManager.SetAllInteractable();
                else
                    SetNewPinYin();
            }
        }
    }

    //用于Unity下调试手写识别结果反馈
    public void TestHWRRec(string results)
    {
        AndroidUtil.Log("识别到的结果:" + results);
        if (results != null && results.Length >= 1)
        {
            AddCharacter(results.Substring(0, 1));
            currentCharacter++;
            if (currentCharacter == contentArray.Length)
                btnManager.SetAllInteractable();
            else
                SetNewPinYin();
        }
    }

    public void ResultJudge(string btnName)
    {
        if (!chineseContent.text.Equals(infos[currentChinese].Content))
        {
            //TODO 错误效果
            FadeInRoleAnim(attackRole, "fail");
//            attackRole.animation.Play("normal");
            AndroidUtil.Toast("输入错误!!!\n" + "实际: " + chineseContent.text + "\n输入: " +
                              infos[currentChinese].Content);
        }
        else
        {
            //TODO 攻击效果
            if ("AttachBtn".Equals(btnName))
            {
                FadeInRoleAnim(attackRole, "attack");
//              attackRole.animation.Play("normal");
                AndroidUtil.Toast("攻击效果!!!");
            }
            else if ("CureBtn".Equals(btnName))
            {
                //TODO 需要修改成对应对象的动画
                FadeInRoleAnim(attackRole, "attack");

                AndroidUtil.Toast("治疗效果!!!");
            }
            else if ("DefensenBtn".Equals(btnName))
            {
                //TODO 需要修改成对应对象的动画
                attackRole.animation.FadeIn("attack", 0.2f, 1);

                AndroidUtil.Toast("防御效果!!!");
            }
        }

        currentChinese++;
        isNewRec = true;
        isCalcTime = false;
    }

    private void JudgeGameOver()
    {
        if (currentChinese == infos.Length)
        {
            //TODO 游戏结束,此方法应该写在动画回调之后，需要修改
            AndroidUtil.Toast("游戏结束");
            GameSetting._instance.SetGameOver(true);
        }
        else
        {
            UpdateChineseInfo(infos[currentChinese]);
        }

        isNewRec = false;
    }

    public void UpdateChineseInfo(ChineseInfo chinese)
    {
        currentCharacter = 0;
        pinYinArray = chinese.Pinyin.Split(' ');
        contentArray = chinese.Content.Split(' ');
        AndroidUtil.Log("拼音: " + GeneralTools.printArray(pinYinArray));
        AndroidUtil.Log("内容: " + GeneralTools.printArray(contentArray));

        chinesePinYin.text = chinese.Pinyin;
        chineseContent.text = "";
        chineseCurrentPinYin.text = pinYinArray[currentCharacter];
    }

    public void AddCharacter(string s)
    {
        if (chineseContent.text.Length > 0)
            chineseContent.text += " " + s;
        else
            chineseContent.text += s;
    }

    public void SetNewPinYin()
    {
        chineseCurrentPinYin.text = pinYinArray[currentCharacter];
    }

    public void ShowDetialContent()
    {
        AndroidUtil.Toast(infos[currentChinese].Detail, 0, 0);
    }

    //动画完成后切换回默认动画
    private void OnAnimationEventHandler(string type, EventObject eventObject)
    {
        if (isNewAnim)
        {
            isNewAnim = false;
            if (!"".Equals(newAnimName))
            {
                StartCoroutine(DelayAnimPlay(attackRole, newAnimName));
                newAnimName = "";
            }
        }
        else
        {
            PlayRoleAnim(attackRole, "normal");
            if (isNewRec)
            {
                JudgeGameOver();
                isNewRec = false;
            }

            isCalcTime = true;
        }
    }

    public void FadeInRoleAnim(UnityArmatureComponent role, String animName)
    {
        if (role.animation.lastAnimationName != "normal")
        {
            isNewAnim = true;
            newAnimName = animName;
        }
        else
        {
            role.animation.FadeIn(animName, 0.2f, 1);
        }
    }

    public void PlayRoleAnim(UnityArmatureComponent role, String animName)
    {
        role.animation.Play(animName, 1);
    }

    IEnumerator DelayAnimPlay(UnityArmatureComponent role, String animName)
    {
        yield return new WaitForSeconds(0.5f);
        PlayRoleAnim(role, animName);
    }
}