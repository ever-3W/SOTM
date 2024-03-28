using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Forms;
using SOTM.MissionControl.Models;

namespace SOTM.MissionControl.Services
{
    public class GameService
    {
        public const string GAME_STORAGE_KEY = "CurrentGame";

        public GenericRepository<Game?> repo = new(GAME_STORAGE_KEY, null);
        public Game? game
        {
            get => this.repo.value;
            set => this.repo.value = value;
        }
    }
}