using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

[FirestoreData]
public class UserData
{
    [FirestoreProperty]
    public string username { get; set; }
    [FirestoreProperty]
    public int bestScore { get; set; }
    [FirestoreProperty]
    public int timestamp { get; set; }
}
