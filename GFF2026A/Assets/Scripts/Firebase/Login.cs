using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;

public class Login : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("接続できた");
                FirebaseApp app = FirebaseApp.DefaultInstance;
                firestore = FirebaseFirestore.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;

                CreateUser("test@gmail.com", "testtest");
            }
            else
            {
                Debug.Log("接続できなかった");
            }
        });

    }

    void CreateUser(string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                FirebaseUser newUser = task.Result.User;
            }
            else
            {
                Debug.Log("接続できなかった");
            }
        });
    }
}
