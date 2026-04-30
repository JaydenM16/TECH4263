using System.Net.Http.Json;
using System.Text.Json;
using EquipmentDashboard.Models;

namespace EquipmentDashboard
{
    public partial class MainForm : Form
    {
        // HTTP Client ───────────────────────────────────────────────────────────────────────────────
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7226")  // EquipmentAPI Url
        };

        // State ────────────────────────────────────────────────────────────────────────────────────
        private List<EquipmentResponseDto> _equipment = new();

        public MainForm()
        {
            InitializeComponent();
            _ = LoadEquipmentAsync();
        }

        // Load Students From API ────────────────────────────────────────────────────────────────────
        private async Task LoadEquipmentAsync()
        {
            try
            {
                SetStatus("Loading...", Color.Gray);

                var response = await _httpClient.GetAsync("/equipments");

                if (!response.IsSuccessStatusCode)
                {
                    SetStatus($"Error: {response.StatusCode}", Color.Red);
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                _equipment = JsonSerializer.Deserialize<List<EquipmentResponseDto>>(json,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                            ?? new List<EquipmentResponseDto>();

                lstEquipment.Items.Clear();
                foreach (var s in _equipment)
                    lstEquipment.Items.Add(s.Name);

                SetStatus("", Color.Gray);
            }
            catch (HttpRequestException)
            {
                SetStatus("Cannot connect to EquipmentAPI. Is it running?", Color.Red);
            }
        }
        // List Selection Changed ───────────────────────────────────────────────────────────────────
        private void lstStudents_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = lstEquipment.SelectedIndex;

            if (index < 0 || index >= _equipment.Count)
            {
                lblNoSelection.Visible = true;
                pnlDetailFields.Visible = false;
                return;
            }

            var equipment = _equipment[index];
            lblIdValue.Text = equipment.Id.ToString();
            lblNameValue.Text = equipment.Name;
            lblCategoryValue.Text = equipment.Category;
            lblStatusValue.Text = equipment.Status;
            lblLocationValue.Text = equipment.Location;



            lblNoSelection.Visible = false;
            pnlDetailFields.Visible = true;
        }

        // Create Button Clicked ─────────────────────────────────────────────────────────────────────
        private async void btnCreate_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string category = txtCategory.Text.Trim();
            string status = txtStatus.Text.Trim();
            string location = txtLocation.Text.Trim();


            if (string.IsNullOrEmpty(name))
            {
                SetStatus("Name is required.", Color.OrangeRed);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrEmpty(status))
            {
                SetStatus("Status is required.", Color.OrangeRed);
                txtStatus.Focus();
                return;
            }

            if (string.IsNullOrEmpty(location))
            {
                SetStatus("Location is required.", Color.OrangeRed);
                txtLocation.Focus();
                return;
            }

            var dto = new CreateEquipmentDto { Name = name, Category = category, Status = status, Location = location };

            try
            {
                btnCreate.Enabled = false;
                SetStatus("Creating...", Color.Gray);

                var response = await _httpClient.PostAsJsonAsync("/equipments", dto);

                if (response.IsSuccessStatusCode)
                {
                    txtName.Clear();
                    txtCategory.Clear();
                    txtStatus.Clear();
                    txtLocation.Clear();
                    await LoadEquipmentAsync();
                    SetStatus($"Equipment '{name}' created successfully.", Color.SeaGreen);
                }
                else
                {
                    SetStatus($"Failed: {response.StatusCode}", Color.Red);
                }
            }
            catch (HttpRequestException)
            {
                SetStatus("Cannot connect to EquipmentAPI.", Color.Red);
            }
            finally
            {
                btnCreate.Enabled = true;
            }
        }

        // Status Helper ───────────────────────────────────────────────────────────────────────────────
        private void SetStatus(string message, Color color)
        {
            lblStatusMsg.Text = message;
            lblStatusMsg.ForeColor = color;
        }

    }
}

