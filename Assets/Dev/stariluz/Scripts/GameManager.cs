using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;


namespace Stariluz
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public delegate void GameState();

        public GameState OnGame, OnUI;

        [Header("Other Managers")]
        [SerializeField] VoiceNarrativeManager voiceNarrativeManager;

        [Header("Scene Navigation")]
        public SceneTransitionManager transitionManager;
        public string nextScene = "TitleScreen";

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

        [Header("UI First Selected")]
        public GameObject playUIFirstButton;
        public GameObject pauseMenuFirstButton;
        public GameObject settingsMenuFirstButton;
        public GameObject deathMenuFirstButton;
        public GameObject confirmRebootFirstButton;
        public GameObject confirmExitGameFirstButton;

        public UIState cancelState = UIState.InPauseScreen;

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
            if (inputActions != null)
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
            UIAudioManager.Instance.PlayNextSound();
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
            {
                voiceNarrativeManager.AudioSource.UnPause();
                OnGame?.Invoke();
            }
            else
            {
                voiceNarrativeManager.AudioSource.Pause();
                OnUI?.Invoke();
            }
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

            // Esperar un frame antes de establecer el botón seleccionado
            StartCoroutine(SetInitialSelectedButton(newState));
            // RestoreSelectedButton(newState);
        }

        private IEnumerator SetInitialSelectedButton(UIState newState)
        {
            yield return null; // espera un frame

            GameObject toSelect = null;

            switch (newState)
            {
                case UIState.InPlayScreen:
                    toSelect = playUIFirstButton;
                    break;

                case UIState.InPauseScreen:
                    toSelect = pauseMenuFirstButton;
                    break;

                case UIState.InSettingsScreen:
                    toSelect = settingsMenuFirstButton;
                    break;

                case UIState.InDeathScreen:
                    toSelect = deathMenuFirstButton;
                    break;

                case UIState.InConfirmRebootScreen:
                    toSelect = confirmRebootFirstButton;
                    break;

                case UIState.InConfirmExitGameScreen:
                    toSelect = confirmExitGameFirstButton;
                    break;
            }

            EventSystem.current.SetSelectedGameObject(null); // limpia selección previa
            if (toSelect != null && toSelect.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(toSelect);
            }
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
                    switch (cancelState)
                    {
                        case UIState.InPauseScreen:
                            OpenPause();
                            break;
                        case UIState.InSettingsScreen:
                            OpenSettings();
                            break;
                        case UIState.InDeathScreen:
                            // For debug purposes activate death screen sound
                            break;
                    }
                    break;

                case UIState.InDeathScreen:
                    ReturnHome();
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
            UIAudioManager.Instance.PlayRestartSound();
            Time.timeScale = 1f;
            // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            transitionManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ExitGame()
        {
            Debug.Log("Exiting game...");
            Application.Quit();
        }

        public void LoadScene(string sceneName)
        {
            transitionManager.LoadScene(sceneName);
        }

        public void LoadNextScene()
        {
            UIAudioManager.Instance.PlaySuccessSound();
            LoadScene(nextScene);
        }

        public void ReturnHome()
        {
            UIAudioManager.Instance.PlayLeaveSound();
            LoadScene("TitleScreen");
        }
        public void OpenSettings()
        {
            UIAudioManager.Instance.PlayNextSound();
            LoadState(UIState.InSettingsScreen);
        }
        public void OpenConfirmReboot()
        {
            UIAudioManager.Instance.PlayNextSound();
            LoadState(UIState.InConfirmRebootScreen);
        }
        public void OpenConfirmExitGame()
        {
            UIAudioManager.Instance.PlayNextSound();
            LoadState(UIState.InConfirmExitGameScreen);
        }
        public void OpenPause()
        {
            UIAudioManager.Instance.PlayPauseSound();
            LoadState(UIState.InPauseScreen);
        }
        public void ResumeGame()
        {
            ReturnState();
        }
        public void Die()
        {
            ChangeState(UIState.InDeathScreen);
        }
    }

}
