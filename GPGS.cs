using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Added Headers
using GooglePlayGames;
using GooglePlayGames.BasicApi;

using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.BasicApi.Events;

using UnityEngine.SocialPlatforms;
//
using System;
// Testing local save and load
using System.IO;

using TMPro;
#region About GPGS
/*
 * GPGS Version 11.01 (on Mar 24, 2022)
 * https://github.com/playgameservices/play-games-plugin-for-unity
 * 
 * need to Setting Up Google Play Games Services / inorder to use GPGS
 * https://developers.google.com/games/services/console/enabling

    use Unity's Social Platform instead of Play Games Services
    https://docs.unity3d.com/Manual/net-SocialAPI.html

 * GPGSBinder Created by DevCol [ in9 ] (on Nov 9, 2023) ver.1
 * Last edit: (on Nov 29, 2023)
 * 
 */
#endregion


// GPGS only for android NO! iOS
// set it from UNITY Build Setting

// *** to check achievement need to change setting at Play Games / at Profile, and privacy / change game activity setting

// * How to Reset Leaderboard: at Google Play Console > Play Games Services > Setup and management > Leaderboards > reset

// GPGSIds automatically create and edit by Google Play Console (with proper Play Console resource)

#if UNITY_ANDROID

public class GPGS : MonoBehaviour
{

    private string savedFileName = "NewSaveData.text";

    private bool isSaving = true;

    public TMP_Text scoreText;

    int score = 0;
    string log;


    public void Start()
    {
        Authentication();
    }

    void Authentication()
    {
        PlayGamesPlatform.Activate();

        Social.localUser.Authenticate(ProcessAuthentication);
    }

    void ProcessAuthentication(bool success)
    {
        if (success)
        {
            string message = "Authenticated, checking achievements";

            Debug.Log(message);
            log = message;

        }
        else
        {
            string message = "Failed to authenticate";

            Debug.Log(message);
            log = message;
        }
    }


    void OnGUI()
    {
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * 3);

        //
        if (GUILayout.Button("ClearLog"))
            log = "Cleared";

        GUILayout.Label(log);
        //

        #region Save/Load/Delete
        if (GUILayout.Button("ShowSelectUI"))
        {
            ShowSelectUI();
        }

        if (GUILayout.Button("Save"))
        {
            log = "Save";

            Save();
        }

        if (GUILayout.Button("Load"))
        {
            log = "Load";

            Load();
        }

        if (GUILayout.Button("Delete"))
        {
            log = "Delete";

            DeleteGameData(savedFileName);
        }

        #endregion

        if (GUILayout.Button("Up"))
        {
            log = "Up";

            score += 1;

        }

        if (GUILayout.Button("Down"))
        {
            log = "Down";
            score -= 1;
        }



        #region Achievements
        if (GUILayout.Button("Show AchievementsUI"))
        {
            Social.ShowAchievementsUI();
        }

        //
        if (GUILayout.Button("unlock Achievements one"))
        {
            /*
              Notice that according to the expected behavior of Social.ReportProgress,
             a progress of 0.0f means revealing the achievement
             and a progress of 100.0f means unlocking the achievement.
             Therefore, to reveal an achievement (that was previously hidden) without unlocking it,
             simply call Social.ReportProgress with a progress of 0.0f.

              */

            Social.ReportProgress(GPGSIds.achievement_one, 100.0f, (bool success) => {
                // handle success or failure
            });


        }

        if (GUILayout.Button("reveal hidden achievement"))
        {
            Social.ReportProgress(GPGSIds.achievement_two, 0.0f, (bool success) => {
                // handle success or failure
            });
        }


        if (GUILayout.Button("increment achievement"))
        {
             /*
                If your achievement is incremental,the Play Games implementation of
                    Social.ReportProgress will try to behave as closely as possible to
                    the expected behavior according to Unity's social API,
                    but may not be exact.

                For this reason, we recommend that you do not use Social.ReportProgress for incremental achievements.
                    Instead, use the PlayGamesPlatform.IncrementAchievement method, which is a Play Games extension.

            */

            PlayGamesPlatform.Instance.IncrementAchievement(
                 GPGSIds.achievement_three, 1 , (bool success) => {
                  // handle success or failure
              });

        }


        #endregion

        #region Leaderboard
        if (GUILayout.Button("Show LeaderboardUI"))
        {
            Social.ShowLeaderboardUI();

            //If you wish to show a particular leaderboard instead of all leaderboards
            //PlayGamesPlatform.Instance.ShowLeaderboardUI("Cfji293fjsie_QA");
        }

        if (GUILayout.Button("Show lead leaderbord"))
        {
            PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard_lead);
        }

        if (GUILayout.Button("Show lead low leaderbord"))
        {
            PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard_lead_low);
        }

        if (GUILayout.Button("post a score to a lead"))
        {
            Social.ReportScore(score, GPGSIds.leaderboard_lead, (bool success) => {

                // handle success or failure

                if (success)
                {
                    string message = "post a score suc";

                    Debug.Log(message);

                    log = message;
                }
                else
                {
                    string message = "post a score fail";

                    Debug.Log(message);

                    log = message;
                }
            });

        }

        // * How to Reset Leaderboard: at Google Play Console > Play Games Services > Setup and management > Leaderboards > reset
             
        #endregion

    }

    public void Update()
    {
        scoreText.text = score.ToString();
    }


    #region Displaying saved games UI


    void ShowSelectUI()
    {
        uint maxNumToDisplay = 5;
        bool allowCreateNew = false;
        bool allowDelete = true;

        //
        string showSavedGameTitle = "Select Saved Game";

        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.ShowSelectSavedGameUI(showSavedGameTitle ,
            maxNumToDisplay,
            allowCreateNew,
            allowDelete,
            OnSavedGameSelected);
    }

    public void OnSavedGameSelected(SelectUIStatus status, ISavedGameMetadata game)
    {
        if (status == SelectUIStatus.SavedGameSelected)
        {
            // handle selected game save
            Debug.Log("load from OnSavedGameSelected");
            log = (game.Filename);

            string message = "load from OnSavedGameSelected";

            Debug.Log(message + " " + game.Filename);

            log = message + " " + game.Filename;

            Load();
        }
        else
        {
        }

    }
    #endregion

    // Save or Load > OpenSavedGame > OnSavedGameOpened > SaveGame or LoadGameData

    #region Save / Load / Delete

    public void Save()
    {
        isSaving = true;

        OpenSavedGame(savedFileName);
    }

    public void Load()
    {
        isSaving = false;

        OpenSavedGame(savedFileName);
    }
    

    // ======

    void OpenSavedGame(string filename)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.OpenWithAutomaticConflictResolution(filename, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime, OnSavedGameOpened);
    }

    
    public void OnSavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // handle reading or writing of saved game.

            // saving
            if(isSaving)
            {
                Debug.Log("OnSavedGameOpened Saving");

                byte[] myData = System.Text.ASCIIEncoding.ASCII.GetBytes(score.ToString());

                SaveGame(game, myData);
            }

            // loading
            else
            {
                Debug.Log("OnSavedGameOpened Loading");

                LoadGameData(game);
            }
        }
        else
        {
            // handle error
        }
    }

    void SaveGame(ISavedGameMetadata game, byte[] savedData)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

        SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
        builder = builder
            .WithUpdatedDescription("Saved game at " + DateTime.Now);

        SavedGameMetadataUpdate updatedMetadata = builder.Build();

        //
        savedGameClient.CommitUpdate(game, updatedMetadata, savedData, OnSavedGameWritten);
    }

    public void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // handle reading or writing of saved game.
            Debug.Log("Success to save to the cloud");
        }
        else
        {
            // handle error
            Debug.Log("Failed to save to the cloud");
        }
    }


    //==============

    void LoadGameData(ISavedGameMetadata game)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.ReadBinaryData(game, OnSavedGameDataRead);
    }

    public void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] data)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // handle processing the byte array data
            Debug.Log("SavedGameRequestStatus.Success");

            //string loadedData = System.Text.Encoding.UTF8.GetString(data);

            string loadedData = System.Text.ASCIIEncoding.ASCII.GetString(data);

            score = Int32.Parse(loadedData);
            

        }

        else if (status == SavedGameRequestStatus.AuthenticationError)
        {

            Debug.Log("Error: AuthenticationError");
            

        }
        else if (status == SavedGameRequestStatus.BadInputError)
        {
            Debug.Log("Error: BadInputError");
                       

        }
        else if (status == SavedGameRequestStatus.InternalError)
        {
            Debug.Log("Error: InternalError");
            

        }
        else if (status == SavedGameRequestStatus.TimeoutError)
        {
            Debug.Log("Error: TimeoutError");
            

        }
        else
        {
            Debug.Log("Error: UNKOWN");

        }
    }


    void DeleteGameData(string filename)
    {
        // Open the file to get the metadata.
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.OpenWithAutomaticConflictResolution(filename, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime, DeleteSavedGame);
    }

    public void DeleteSavedGame(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.Delete(game);
        }
        else
        {
            // handle error
        }
    }
    //==========================



    #endregion
   
}

#endif // UNITY_ANDROID
