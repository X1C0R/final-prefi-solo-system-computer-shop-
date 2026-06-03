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
        [JsonProperty("status")] public string Status { get; set; }  // available | in_use | reserved
        [JsonProperty("current_customer")] public string CurrentCustomer { get; set; }
        [JsonProperty("session_start")] public DateTime? SessionStart { get; set; }
        [JsonProperty("hourly_rate")] public decimal HourlyRate { get; set; }
        [JsonProperty("notes")] public string Notes { get; set; }
    }

    public class SupabaseService
    {
        private static readonly HttpClient _client = new HttpClient();

        // ✅ Replace these with your actual Supabase URL and anon key
        private const string BASE_URL = "https://jttyurjxcroyzhunfvhj.supabase.co";
        private const string API_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imp0dHl1cmp4Y3JveXpodW5mdmhqIiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODA1MDIzMDQsImV4cCI6MjA5NjA3ODMwNH0.g2BVEhFvQVD8tKeCk3YozoHRB-joMD4rsvampu0mugg";

        static SupabaseService()
        {
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("apikey", API_KEY);
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // ── Fetch all computers ordered by number ─────────────────────────
        public static async Task<List<Computer>> GetComputersAsync()
        {
            var url = $"{BASE_URL}/rest/v1/computers?select=*&order=computer_number.asc";
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Computer>>(json)
                   ?? new List<Computer>();
        }

        // ── Start a session (set status → in_use) ─────────────────────────
        public static async Task StartSessionAsync(string computerId, string customerName)
        {
            // 1. Update computer status
            var patch = new
            {
                status = "in_use",
                current_customer = customerName,
                session_start = DateTime.UtcNow.ToString("o"),
            };

            var patchJson = JsonConvert.SerializeObject(patch);
            var patchReq = new HttpRequestMessage(new HttpMethod("PATCH"),
                $"{BASE_URL}/rest/v1/computers?id=eq.{computerId}");
            patchReq.Content = new StringContent(patchJson, Encoding.UTF8, "application/json");
            patchReq.Headers.Add("Prefer", "return=minimal");
            await _client.SendAsync(patchReq);

            // 2. Insert session record
            var session = new
            {
                computer_id = computerId,
                customer_name = customerName,
                started_at = DateTime.UtcNow.ToString("o"),
            };
            var sessionJson = JsonConvert.SerializeObject(session);
            var postReq = new HttpRequestMessage(HttpMethod.Post,
                $"{BASE_URL}/rest/v1/computer_sessions");
            postReq.Content = new StringContent(sessionJson, Encoding.UTF8, "application/json");
            postReq.Headers.Add("Prefer", "return=minimal");
            await _client.SendAsync(postReq);
        }

        // ── End a session (set status → available) ────────────────────────
        public static async Task EndSessionAsync(Computer computer)
        {
            if (computer.SessionStart == null) return;

            var elapsed = DateTime.UtcNow - computer.SessionStart.Value;
            var hoursUsed = Math.Max(Math.Round(elapsed.TotalHours, 2), 0.1);
            var totalCharge = Math.Round((decimal)hoursUsed * computer.HourlyRate, 2);

            // 1. Close session record
            var sessionPatch = new
            {
                ended_at = DateTime.UtcNow.ToString("o"),
                hours_used = hoursUsed,
                total_charge = totalCharge,
            };
            var spJson = JsonConvert.SerializeObject(sessionPatch);
            var spReq = new HttpRequestMessage(new HttpMethod("PATCH"),
                $"{BASE_URL}/rest/v1/computer_sessions" +
                $"?computer_id=eq.{computer.Id}&ended_at=is.null");
            spReq.Content = new StringContent(spJson, Encoding.UTF8, "application/json");
            spReq.Headers.Add("Prefer", "return=minimal");
            await _client.SendAsync(spReq);

            // 2. Reset computer
            var reset = new
            {
                status = "available",
                current_customer = (string)null,
                session_start = (string)null,
            };
            var rJson = JsonConvert.SerializeObject(reset);
            var rReq = new HttpRequestMessage(new HttpMethod("PATCH"),
                $"{BASE_URL}/rest/v1/computers?id=eq.{computer.Id}");
            rReq.Content = new StringContent(rJson, Encoding.UTF8, "application/json");
            rReq.Headers.Add("Prefer", "return=minimal");
            await _client.SendAsync(rReq);
        }

        // ── Reserve a computer ────────────────────────────────────────────
        public static async Task ReserveAsync(string computerId, string customerName)
        {
            var patch = new
            {
                status = "reserved",
                current_customer = customerName,
                session_start = (string)null,
            };
            var json = JsonConvert.SerializeObject(patch);
            var req = new HttpRequestMessage(new HttpMethod("PATCH"),
                $"{BASE_URL}/rest/v1/computers?id=eq.{computerId}");
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            req.Headers.Add("Prefer", "return=minimal");
            await _client.SendAsync(req);
        }

        // ── Cancel reservation / free the computer ─────────────────────────
        public static async Task FreeComputerAsync(string computerId)
        {
            var patch = new
            {
                status = "available",
                current_customer = (string)null,
                session_start = (string)null,
            };
            var json = JsonConvert.SerializeObject(patch);
            var req = new HttpRequestMessage(new HttpMethod("PATCH"),
                $"{BASE_URL}/rest/v1/computers?id=eq.{computerId}");
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            req.Headers.Add("Prefer", "return=minimal");
            await _client.SendAsync(req);
        }


    }
}