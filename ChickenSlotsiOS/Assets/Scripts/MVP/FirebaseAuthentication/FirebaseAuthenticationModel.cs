using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseAuthenticationModel
{
    public event Action<string> OnChangeUser;

    public event Action OnSignIn_Action;
    public event Action<string> OnSignInError_Action;

    public event Action OnSignUp_Action;
    public event Action<string> OnSignUpError_Action;

    public event Action OnSignOut_Action;

    public event Action OnDeleteAccount_Action;

    public event Action OnEnterRegisterLoginSuccess;
    public event Action<string> OnEnterRegisterLoginError;


    private FirebaseAuth auth;

    private readonly Regex mainRegex = new("^[a-zA-Z0-9._]*$");
    private readonly Regex invalidRegex = new(@"(\.{2,}|/{2,})");
    private const string URL = "https://dinoipsum.com/api/?format=text&paragraphs=1&words=1";

    public FirebaseAuthenticationModel(FirebaseAuth auth)
    {
        this.auth = auth;
    }

    public void Initialize()
    {
        auth = FirebaseAuth.DefaultInstance;
        //databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public bool CheckUserAuthentication()
    {
        if (auth.CurrentUser != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SignIn(string emailTextValue, string passwordTextValue)
    {
        Coroutines.Start(SignInCoroutine(emailTextValue, passwordTextValue));
    }

    public void SignUp(string emailTextValue)
    {
        Coroutines.Start(SignUpCoroutine(emailTextValue + "@gmail.com", "123456"));
    }

    public void SignOut()
    {
        auth.SignOut();
        OnSignOut_Action?.Invoke();
        OnChangeUser?.Invoke(auth.CurrentUser.UserId);
    }

    public void DeleteAccount()
    {
        OnDeleteAccount_Action?.Invoke();
        Coroutines.Start(DeleteAuth_Coroutine());
    }

    public void ChangeEnterLoginValue(string value)
    {
        if (value.Length < 5)
        {
            OnEnterRegisterLoginError?.Invoke("Nickname must be at least 5 characters long");
            return;
        }

        if (!mainRegex.IsMatch(value))
        {
            OnEnterRegisterLoginError?.Invoke("Nickname can only contain english letters, numbers, period and ");
            return;
        }

        if (invalidRegex.IsMatch(value))
        {
            OnEnterRegisterLoginError?.Invoke("Nickname cannot contain consencutive periods and slashes");
            return;
        }

        if (value.EndsWith("."))
        {
            OnEnterRegisterLoginError?.Invoke("Nickname cannot end with a period");
            return;
        }

        OnEnterRegisterLoginSuccess?.Invoke();
    }

    private IEnumerator SignInCoroutine(string emailTextValue, string passwordTextValue)
    {
        Task<AuthResult> task = auth.SignInWithEmailAndPasswordAsync(emailTextValue, passwordTextValue);

        yield return new WaitUntil(() => task.IsCompleted);
        yield return null;

        if (task.Exception != null)
        {
            OnSignInError_Action?.Invoke(task.Exception.Message);
            yield break;
        }

        //OnChangeUser?.Invoke();
        OnChangeUser?.Invoke(auth.CurrentUser.UserId);
        OnSignIn_Action?.Invoke();
    }

    private IEnumerator SignUpCoroutine(string emailTextValue, string passwordTextValue)
    {
        var task = auth.CreateUserWithEmailAndPasswordAsync(emailTextValue, passwordTextValue);

        yield return new WaitUntil(predicate: () => task.IsCompleted);
        yield return null;

        if (task.Exception != null)
        {
            Debug.Log("�� ������� ������� �������");
            OnSignUpError_Action?.Invoke(task.Exception.Message);
            yield break;
        }

        Debug.Log("������� ������");
        OnChangeUser?.Invoke(auth.CurrentUser.UserId);
        OnSignUp_Action?.Invoke();

    }

    private IEnumerator DeleteAuth_Coroutine()
    {
        var task = auth.CurrentUser.DeleteAsync();

        yield return new WaitUntil(predicate: () => task.IsCompleted);
        yield return null;

        if (task.Exception != null)
        {
            Debug.Log("������ �������� �������� - " + task.Exception.Message);
            yield break;
        }

        SignOut();
    }
}
