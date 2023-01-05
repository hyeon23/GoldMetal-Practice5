using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject menuCam;
    public GameObject gameCam;
    public Player player;
    public Boss boss;
    public int stage;
    public float playTime;
    public bool isBattle;
    public int enemyCntA;
    public int enemyCntB;
    public int enemyCntC;
    public int enemyCntD;

    public Transform[] enemyZones;
    public GameObject[] enemies;
    public List<int> enemyList;

    //GameManager에선 인게임의 모든 UI요소 가져오기
    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject overPanel;

    public TMP_Text maxScoreTxt;

    public TMP_Text scoreTxt;
    public TMP_Text stageTxt;
    public TMP_Text playTimeTxt;

    public TMP_Text playerHealthTxt;
    public TMP_Text playerAmmoTxt;
    public TMP_Text playerCoinTxt;

    public Image weapon1Img;
    public Image weapon2Img;
    public Image weapon3Img;
    public Image weaponRImg;

    public TMP_Text enemyATxt;
    public TMP_Text enemyBTxt;
    public TMP_Text enemyCTxt;
    public TMP_Text enemyDTxt;

    public RectTransform bossHealthGroup;
    public RectTransform bossHealthBar;
    public TMP_Text curScoreText;
    public TMP_Text bestText;

    public GameObject itemShop;
    public GameObject weaponShop;
    public GameObject startZone;
    

    void Awake()
    {
        enemyList = new List<int>();
        maxScoreTxt.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore"));//(Int)GetInt -> String

        if (PlayerPrefs.HasKey("MaxScore"))
            PlayerPrefs.SetInt("MaxScore", 0);
    }

    public void GameStart()
    {
        menuCam.SetActive(false);
        gameCam.SetActive(true);

        menuPanel.SetActive(false);
        gamePanel.SetActive(true);

        player.gameObject.SetActive(true);
    }

    public void StageStart()
    {
        itemShop.SetActive(false);
        weaponShop.SetActive(false);
        startZone.SetActive(false);

        foreach (Transform zone in enemyZones)
            zone.gameObject.SetActive(true);

        isBattle = true;
        StartCoroutine(InBattle());
    } 
    
    public void GameOver()
    {
        gamePanel.SetActive(false);
        overPanel.SetActive(true);
        curScoreText.text = scoreTxt.text;

        int maxScore = PlayerPrefs.GetInt("MaxScore");
        if(player.score > maxScore)
        {
            bestText.gameObject.SetActive(true);
            PlayerPrefs.SetInt("MaxScore", player.score);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public void StageEnd()
    {
        player.transform.position = Vector3.up * 0.8f;

        itemShop.SetActive(true);
        weaponShop.SetActive(true);
        startZone.SetActive(true);

        foreach (Transform zone in enemyZones)
            zone.gameObject.SetActive(false);

        isBattle = false;
        stage++;
    }

    IEnumerator InBattle()
    {
        if(stage % 5 == 0)
        {
            enemyCntD++;
            GameObject instantEnemy = Instantiate(enemies[3], enemyZones[3].position, enemyZones[3].rotation);
            Enemy enemy = instantEnemy.GetComponent<Enemy>();
            enemy.target = player.transform;
            enemy.gameManager = this;
            boss = instantEnemy.GetComponent<Boss>();
        }
        else
        {
            for (int index = 0; index < stage; index++)
            {
                int ran = Random.Range(0, 3);
                enemyList.Add(ran);

                switch (ran)
                {
                    case 0:
                        enemyCntA++;
                        break;
                    case 1:
                        enemyCntB++;
                        break;
                    case 2:
                        enemyCntC++;
                        break;
                }
            }

            while (enemyList.Count > 0)
            {
                int ranZone = Random.Range(0, 4);
                GameObject instantEnemy = Instantiate(enemies[enemyList[0]], enemyZones[ranZone].position, enemyZones[ranZone].rotation);
                Enemy enemy = instantEnemy.GetComponent<Enemy>();
                enemy.target = player.transform;
                enemy.gameManager = this;
                enemyList.RemoveAt(0);
                yield return new WaitForSeconds(5f);
            }
        }
        while (enemyCntA + enemyCntB + enemyCntC + enemyCntD > 0)
        {
            yield return null;
        }
        yield return new WaitForSeconds(4f);
        boss = null;
        StageEnd();
    }

    private void Update()
    {
        if (isBattle)
            playTime += Time.deltaTime;//Time.deltaTime을 꾸준히 더해준 것이 곧 PlayTime이 됨
    }

    void LateUpdate()
    {
        scoreTxt.text = string.Format("{0:n0}", player.score);
        stageTxt.text = "STAGE " + stage;

        int hour = (int)(playTime / 3600);
        int minute = (int)((playTime - hour * 3600) / 60);
        int second = (int)(playTime % 60);

        playTimeTxt.text = string.Format("{0:00}", hour) + ":" + string.Format("{0:00}", minute) + ":" + string.Format("{0:00}", second);

        playerHealthTxt.text = player.health + " / " + player.maxHealth;
        playerCoinTxt.text = string.Format("{0:n0}", player.coin);
        if (player.equipWeapon == null)
            playerAmmoTxt.text = "- / " + player.ammo;
        else if(player.equipWeapon.type == Weapon.Type.Melee)
            playerAmmoTxt.text = "- / " + player.ammo;
        else
            playerAmmoTxt.text = player.equipWeapon.curAmmo + " / " + player.ammo;

        //삼항 연산자 간단한 해결
        weapon1Img.color = new Color(1, 1, 1, player.hasWeapons[0] ? 1 : 0);
        weapon2Img.color = new Color(1, 1, 1, player.hasWeapons[1] ? 1 : 0);
        weapon3Img.color = new Color(1, 1, 1, player.hasWeapons[2] ? 1 : 0);
        weaponRImg.color = new Color(1, 1, 1, player.hasGrenades > 0 ? 1 : 0);

        enemyATxt.text = enemyCntA.ToString();
        enemyBTxt.text = enemyCntB.ToString();
        enemyCTxt.text = enemyCntC.ToString();
        enemyDTxt.text = enemyCntD.ToString();

        if (boss != null)
        {
            bossHealthGroup.gameObject.SetActive(true);
            bossHealthBar.localScale = new Vector3((float)boss.curHelath / boss.maxHealth, 1, 1);
        }
        else
        {
            bossHealthGroup.gameObject.SetActive(false);
        }
    }
}
