using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Stariluz
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public delegate void GameState();

        public GameState OnGame, OnUI;

        [Header("UI Canvas")]
        public GameObject canvas;
        public GameObject UIModeBackground;

        [Header("UI Panels")]
        public GameObject playUI;
        public GameObject pauseMenuUI;
        public GameObject settingsMenuUI;
        public GameObject deathUI;
        public GameObject confirmRebootMenuUI;
        public GameObject confirmExitGameMenuUI;
        private UIState _currentState;
        public UIState CurrentState
        {
            get
            {
                return _currentState;
            }
        }

        private Stack<UIState> stateHistory = new Stack<UIState>();

        [HideInInspector]
        protected InputActions _inputActions;
        public InputActions inputActions
        {
            get
            {
                return _inputActions;
            }
        }

        [HideInInspector]
        protected InputActions.UIActions _uiInput;
        public InputActions.UIActions uiInput
        {
            get
            {
                return _uiInput;
            }
        }

        [HideInInspector]
        protected InputActions.PlayerActions _playerInput;
        public InputActions.PlayerActions playerInput
        {
            get
            {
                return _playerInput;
            }
        }


        void Awake()
        {
            // Ensure only one instance exists
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // DontDestroyOnLoad(gameObject);

            _inputActions = new InputActions();
            _uiInput = inputActions.UI;
            _playerInput = inputActions.Player;

        }

        private void OnEnable()
        {
            inputActions.Enable();
            uiInput.Cancel.performed += ManageUICancelAction;
        }
        private void OnDisable()
        {
            if (inputActions!=null)
            {
                inputActions.Disable();
                uiInput.Cancel.performed -= ManageUICancelAction;
            }
        }

        void Start()
        {
            ChangeState(UIState.InPlayScreen);
        }
        public void LoadState(UIState newState)
        {
            stateHistory.Push(CurrentState);
            ChangeState(newState);
        }
        public void ReturnState()
        {
            if (stateHistory.Count > 0)
            {
                var previousState = stateHistory.Pop();
                ChangeState(previousState);
            }
        }

        public void ChangeState(UIState newState)
        {
            DisableAllUI();

            if (UIModeBackground)
                UIModeBackground.SetActive(newState != UIState.InPlayScreen);


            if (newState == UIState.InPlayScreen)
                OnGame?.Invoke();
            else
                OnUI?.Invoke();

            switch (newState)
            {
                case UIState.InPlayScreen:
                    Time.timeScale = 1f;
                    if (playUI) playUI.SetActive(true);
                    break;

                case UIState.InPauseScreen:
                    Time.timeScale = 0f;
                    if (pauseMenuUI) pauseMenuUI.SetActive(true);
                    break;

                case UIState.InSettingsScreen:
                    if (settingsMenuUI) settingsMenuUI.SetActive(true);
                    break;

                case UIState.InDeathScreen:
                    Time.timeScale = 0f;
                    if (deathUI) deathUI.SetActive(true);
                    break;

                case UIState.InConfirmRebootScreen:
                    if (confirmRebootMenuUI) confirmRebootMenuUI.SetActive(true);
                    break;

                case UIState.InConfirmExitGameScreen:
                    if (confirmExitGameMenuUI) confirmExitGameMenuUI.SetActive(true);
                    break;
            }

            _currentState = newState;
        }

        private void DisableAllUI()
        {
            if (playUI) playUI.SetActive(false);
            if (pauseMenuUI) pauseMenuUI.SetActive(false);
            if (settingsMenuUI) settingsMenuUI.SetActive(false);
            if (deathUI) deathUI.SetActive(false);
            if (confirmRebootMenuUI) confirmRebootMenuUI.SetActive(false);
            if (confirmExitGameMenuUI) confirmExitGameMenuUI.SetActive(false);
        }

        private void ManageUICancelAction(InputAction.CallbackContext context)
        {

            switch (CurrentState)
            {
                case UIState.InPlayScreen:
                    OpenPause();
                    break;

                case UIState.InDeathScreen:
                    // Navigate to start screen
                    break;

                case UIState.InPauseScreen:
                case UIState.InSettingsScreen:
                case UIState.InConfirmRebootScreen:
                case UIState.InConfirmExitGameScreen:
                    ReturnState();
                    break;
            }

        }

        public void RestartScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ExitGame()
        {
            Debug.Log("Exiting game...");
            Application.Quit();
        }

        public void OpenSettings() => LoadState(UIState.InSettingsScreen);
        public void OpenConfirmReboot() => LoadState(UIState.InConfirmRebootScreen);
        public void OpenConfirmExitGame() => LoadState(UIState.InConfirmExitGameScreen);
        public void OpenPause() => LoadState(UIState.InPauseScreen);
        public void ResumeGame() => ReturnState();
        public void Die() => ChangeState(UIState.InDeathScreen);
    }

}
