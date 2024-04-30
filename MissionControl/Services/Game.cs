using Blazored.LocalStorage;
using SOTM.MissionControl.Models;
using SOTM.Shared.Models;

namespace SOTM.MissionControl.Services
{
    public class GameService
    {
        public const string GAME_STORAGE_KEY = "CurrentGame";

        private readonly GenericRepository<Game?> repo = new(GAME_STORAGE_KEY, null);
        public Func<ILocalStorageService, Task> Save;
        public Func<ILocalStorageService, Task<Game?>> Load;

        public GameService()
        {
            this.Save = this.repo.Save;
            this.Load = this.repo.Load;
        }

        public void NewGame(Game game)
        {
            this.repo.value = game;
        }

        public void ClearGame()
        {
            this.repo.value = null;
        }

        public bool GameExists => this.repo.value is not null;

        public bool TryGetGame(out Game? game)
        {
            if (this.repo.value is not null)
            {
                game = this.repo.value;
                return true;
            }
            game = null;
            return false;
        }

        public GlobalIdentifier? GameVillain => this.repo.value?.draft.villains.ElementAtOrDefault(0);
        public GlobalIdentifier? GameEnvironment => this.repo.value?.draft.environments.ElementAtOrDefault(0);

        public bool GameHasStarted => (this.repo.value is not null) && (this.repo.value.state == GameState.PLAY);

        public void StartGame(IEnumerable<GlobalIdentifier> pickedHeroes)
        {
            if (this.repo.value is not null)
            {
                this.repo.value.Pick(DeckKind.VILLAIN, this.repo.value.draft.villains[0], 0);
                this.repo.value.Pick(DeckKind.ENVIRONMENT, this.repo.value.draft.environments[0], 0);
                foreach ((GlobalIdentifier identifier, int index) in pickedHeroes.Select((item, i) => (item, i)))
                {
                    this.repo.value.Pick(DeckKind.HERO, identifier, index);
                }
                this.repo.value.state = GameState.PLAY;
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(this.repo.value));
            }
        }
        public Game? EndGameWin()
        {
            if (this.repo.value is not null)
            {
                this.repo.value.state = GameState.WIN;
            }
            return this.repo.value;
        }
        public Game? EndGameLoss()
        {
            if (this.repo.value is not null)
            {
                this.repo.value.state = GameState.LOSS;
            }
            return this.repo.value;
        }
    }
}
