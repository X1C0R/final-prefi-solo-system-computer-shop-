using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ComputerDashboard
{
    public class Computer
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("computer_number")] public int ComputerNumber { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("current_customer")] public string CurrentCustomer { get; set; }
        [JsonProperty("session_start")] public DateTime? SessionStart { get; set; }
        [JsonProperty("hourly_rate")] public decimal HourlyRate { get; set; }
        [JsonProperty("notes")] public string Notes { get; set; }
    }

    public class Member
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("username")] public string Username { get; set; }
        [JsonProperty("full_name")] public string FullName { get; set; }
        [JsonProperty("password")] public string Password { get; set; }
        [JsonProperty("total_seconds_used")] public int TotalSecondsUsed { get; set; }

        // NEW COLUMN: Tracks leftover time credits safely
        [JsonProperty("time_balance_seconds")] public int TimeBalanceSeconds { get; set; }
        [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }
    }

    public class MemberSession
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("member_id")] public string MemberId { get; set; }
        [JsonProperty("computer_id")] public string ComputerId { get; set; }
        [JsonProperty("started_at")] public DateTime StartedAt { get; set; }
        [JsonProperty("paused_at")] public DateTime? PausedAt { get; set; }
        [JsonProperty("seconds_used")] public int SecondsUsed { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
    }

    public class SupabaseService
    {
        private static readonly HttpClient _client = new HttpClient();
        private const string BASE_URL = "https://jttyurjxcroyzhunfvhj.supabase.co";
        private const string API_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imp0dHl1cmp4Y3JveXpodW5mdmhqIiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODA1MDIzMDQsImV4cCI6MjA5NjA3ODMwNH0.g2BVEhFvQVD8tKeCk3YozoHRB-joMD4rsvampu0mugg";

        static SupabaseService()
        {
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("apikey", API_KEY);
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<List<Computer>> GetComputersAsync()
        {
            var url = $"{BASE_URL}/rest/v1/computers?select=*&order=computer_number.asc";
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Computer>>(json) ?? new List<Computer>();
        }

        public static async Task StartSessionAsync(string computerId, string customerName)
        {
            var patch = new { status = "in_use", current_customer = customerName, session_start = DateTime.UtcNow.ToString("o") };
            var patchJson = JsonConvert.SerializeObject(patch);
            var patchReq = new HttpRequestMessage(new HttpMethod("PATCH"), $"{BASE_URL}/rest/v1/computers?id=eq.{computerId}");
            patchReq.Content = new StringContent(patchJson, Encoding.UTF8, "application/json");
            patchReq.Headers.Add("Prefer", "return=minimal");
            await _client.SendAsync(patchReq);
        }

        public static async Task EndSessionAsync(Computer computer)
        {
            if (computer.SessionStart == null) return;
            var reset = new { status = "available", current_customer = (string)null, session_start = (string)null };
            var rJson = JsonConvert.SerializeObject(reset);
            var rReq = new HttpRequestMessage(new HttpMethod("PATCH"), $"{BASE_URL}/rest/v1/computers?id=eq.{computer.Id}");
            rReq.Content = new StringContent(rJson, Encoding.UTF8, "application/json");
            rReq.Headers.Add("Prefer", "return=minimal");
            await _client.SendAsync(rReq);
        }

        public static async Task ReserveAsync(string computerId, string customerName)
        {
            var patch = new { status = "reserved", current_customer = customerName, session_start = (string)null };
            var json = JsonConvert.SerializeObject(patch);
            var req = new HttpRequestMessage(new HttpMethod("PATCH"), $"{BASE_URL}/rest/v1/computers?id=eq.{computerId}");
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            req.Headers.Add("Prefer", "return=minimal");
            await _client.SendAsync(req);
        }

        public static async Task FreeComputerAsync(string computerId)
        {
            var patch = new { status = "available", current_customer = (string)null, session_start = (string)null };
            var json = JsonConvert.SerializeObject(patch);
            var req = new HttpRequestMessage(new HttpMethod("PATCH"), $"{BASE_URL}/rest/v1/computers?id=eq.{computerId}");
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            req.Headers.Add("Prefer", "return=minimal");
            await _client.SendAsync(req);
        }

        public static async Task<List<Member>> GetMembersAsync()
        {
            var url = $"{BASE_URL}/rest/v1/members?select=*&order=created_at.desc";
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Member>>(json) ?? new List<Member>();
        }

        public static async Task AddMemberAsync(string fullName, string username, string password)
        {
            var body = new
            {
                full_name = fullName,
                username = username,
                password = password,
                total_seconds_used = 0,
                time_balance_seconds = 0,
                created_at = DateTime.UtcNow.ToString("o")
            };
            var json = JsonConvert.SerializeObject(body);
            var req = new HttpRequestMessage(HttpMethod.Post, $"{BASE_URL}/rest/v1/members");
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            req.Headers.Add("Prefer", "return=minimal");
            await _client.SendAsync(req);
        }

        // NEW: Updates a member's saved time balance directly in Supabase
        public static async Task UpdateMemberBalanceAsync(string memberId, int balanceSeconds)
        {
            var patch = new { time_balance_seconds = balanceSeconds };
            var json = JsonConvert.SerializeObject(patch);
            var req = new HttpRequestMessage(new HttpMethod("PATCH"), $"{BASE_URL}/rest/v1/members?id=eq.{memberId}");
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            req.Headers.Add("Prefer", "return=minimal");
            await _client.SendAsync(req);
        }

        public static async Task UpdateComputerNotesAsync(string id, string notesValue)
        {
            var patch = new { notes = notesValue };
            var json = JsonConvert.SerializeObject(patch);
            var req = new HttpRequestMessage(new HttpMethod("PATCH"), $"{BASE_URL}/rest/v1/computers?id=eq.{id}");
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            req.Headers.Add("Prefer", "return=minimal");
            await _client.SendAsync(req);
        }

        public static async Task StartMemberSessionAsync(string memberId, string computerId)
        {
            var body = new { member_id = memberId, computer_id = computerId, started_at = DateTime.UtcNow.ToString("o"), status = "active" };
            var json = JsonConvert.SerializeObject(body);
            var req = new HttpRequestMessage(HttpMethod.Post, $"{BASE_URL}/rest/v1/member_sessions");
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            req.Headers.Add("Prefer", "return=minimal");
            await _client.SendAsync(req);
        }

        internal static async Task GetActiveMemberSessionByComputerAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}