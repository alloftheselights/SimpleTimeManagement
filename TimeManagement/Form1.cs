using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using System.Media;

namespace TimeManagement
{
    public partial class Form1 : Form
    {
        private FlowLayoutPanel taskListPanel;
        private Button btnAddTask;
        private Button btnReset; // Declare the reset button
        private List<TaskItem> taskItems;
        private const string SaveFilePath = "tasks.json"; // You can choose your own path
        public event Action TasksChanged;
        private Label lblTotalTimeSum; // Declare a label to display the sum of all total times


        private void SaveTasksToFile()
        {
            var json = JsonConvert.SerializeObject(taskItems, Formatting.Indented);
            File.WriteAllText(SaveFilePath, json);
        }

        private void LoadTasksFromFile()
        {
            if (File.Exists(SaveFilePath))
            {
                var json = File.ReadAllText(SaveFilePath);
                var loadedTasks = JsonConvert.DeserializeObject<List<TaskItem>>(json);
                if (loadedTasks != null)
                {
                    foreach (var taskItem in loadedTasks)
                    {
                     
                        taskItems.Add(taskItem);
                        var taskControl = AddTaskToPanel(taskItem); // Add task and get the control
                        taskControl.SetPriority(taskItem.Priority); // Use the SetPriority method of TaskControl
                    }
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
            LoadTasksFromFile(); // Load tasks when the form loads
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveTasksToFile(); // Save tasks when the form is closing
        }

        private void InitializeCustomComponents()
        {
            taskItems = new List<TaskItem>();

            // Button for adding tasks
            btnAddTask = new Button
            {
                Location = new System.Drawing.Point(600, 20),
                Text = "Add Task",
                AutoSize = true
            };
            btnAddTask.Click += AddTaskButton_Click;
            int spacing = 10;

            // Initialize the Reset Button
            btnReset = new Button
            {
                Location = new System.Drawing.Point(600, 55), // Adjust location as needed
                Text = "Reset",
                AutoSize = true
            };
            btnReset.Click += BtnReset_Click; // Subscribe to the Click event

            // FlowLayoutPanel to display tasks
            taskListPanel = new FlowLayoutPanel
            {
                Location = new System.Drawing.Point(20, btnAddTask.Bottom + spacing),
            Size = new System.Drawing.Size(540, 320), // Adjusted size
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                AllowDrop = true,
                WrapContents = false
            };
            // Adjusting the Y-position of the taskListPanel to be below the btnAddTask
            // This is a spacing between the button and the panel
            // taskListPanel.Location = new System.Drawing.Point(20, btnAddTask.Bottom + spacing);
            taskListPanel.DragEnter += TaskListPanel_DragEnter;
            taskListPanel.DragDrop += TaskListPanel_DragDrop;
            // Initialize the Total Time Sum Label
            lblTotalTimeSum = new Label
            {
                Location = new System.Drawing.Point(608, 90), // Position below the reset button
                AutoSize = true
            };


            //lblTotalTimeSum.BringToFront();

            Controls.Add(btnAddTask);
            Controls.Add(btnReset);
            Controls.Add(lblTotalTimeSum);
            Controls.Add(taskListPanel);
            

           

            TasksChanged += SaveTasksToFile; // Subscribe to the event

            
        }

        public void DeleteTask(TaskItem task)
        {
            // Remove the task from the list
            taskItems.Remove(task);

            // Find and remove the corresponding control
            foreach (Control control in taskListPanel.Controls)
            {
                if (control is TaskControl taskControl && taskControl.Tag == task)
                {
                    taskListPanel.Controls.Remove(taskControl);
                    break;
                }
            }

            // Save the updated list of tasks
            SaveTasksToFile();
        }

        private Control _draggingControl = null;

        private void TaskControl_MouseDown(object sender, MouseEventArgs e)
        {
            // Cast sender to Control to get the control being clicked
            Control control = sender as Control;
            _draggingControl = control; // Store the control that is being dragged
            control.DoDragDrop(control, DragDropEffects.Move); // Initiate the drag-and-drop
        }

        private void TaskControl_MouseMove(object sender, MouseEventArgs e)
        {
            // Only start the drag if the mouse is held down
            if (e.Button == MouseButtons.Left)
            {
                Control control = sender as Control;
                control.DoDragDrop(control, DragDropEffects.Move);
            }
        }

        private void TaskListPanel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move; // Specify the effect as move
        }

        private void TaskListPanel_DragDrop(object sender, DragEventArgs e)
        {
            Point point = taskListPanel.PointToClient(new Point(e.X, e.Y));
            var controlUnderMouse = taskListPanel.GetChildAtPoint(point);

            int index = taskListPanel.Controls.GetChildIndex(controlUnderMouse, false);
            if (index == -1) // If no control found under mouse, add to the end
            {
                index = taskListPanel.Controls.Count - 1;
            }
            // Move the control within the FlowLayoutPanel
            taskListPanel.Controls.SetChildIndex(_draggingControl, index);

            // Move the corresponding TaskItem in the list
            TaskItem itemToMove = (TaskItem)_draggingControl.Tag;
            taskItems.Remove(itemToMove);
            taskItems.Insert(index, itemToMove);

            _draggingControl = null; // Reset the dragging control

            // Save the new order
            SaveTasksToFile();
        }


        private void UpdateTotalTimeSum()
        {
            TimeSpan totalTimeSum = TimeSpan.Zero;
            foreach (var taskItem in taskItems)
            {
                totalTimeSum += taskItem.TotalTime;
            }
            lblTotalTimeSum.Text = totalTimeSum.ToString(@"hh\:mm\:ss");
        }


        private void AddTaskButton_Click(object sender, EventArgs e)
        {
            TaskItem newTask = new TaskItem
            {
                Name = "New Task",
                TotalTime = TimeSpan.FromHours(0), // Example starting time
                TimeRemaining = TimeSpan.FromHours(0)
            };

            taskItems.Add(newTask);
            AddTaskToPanel(newTask);
      
            SaveTasksToFile(); // Save tasks whenever a new task is added

            UpdateTotalTimeSum(); // Update the total time sum label
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            // Optional: Ask the user to confirm the reset
            var confirmation = MessageBox.Show(
                "Are you sure you want to reset all tasks? This action cannot be undone.",
                "Reset Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmation == DialogResult.Yes)
            {
                // Clear all tasks from the list
                taskItems.Clear();

                // Clear all controls from the panel
                taskListPanel.Controls.Clear();

                // Delete the save file if it exists
                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                }
            }

            UpdateTotalTimeSum(); // Update the total time sum label
        }

        private TaskControl AddTaskToPanel(TaskItem task)
        {
            TaskControl taskControl = new TaskControl(task);
            taskControl.Tag = task; // Assign the TaskItem to the Tag property for reference
            taskControl.MouseDown += TaskControl_MouseDown;
            taskControl.MouseMove += TaskControl_MouseMove;
            taskListPanel.Controls.Add(taskControl);

            UpdateTotalTimeSum(); // Update the total time sum label
            return taskControl; // Return the created control
        }



        // Nested TaskControl class
        private class TaskControl : UserControl
        {
            private Label lblTimeRemaining;
            private Label lblTotalRequired;
            private TextBox txtTaskName;
            private Button btnStart;
            private Button btnStop;
            private System.Windows.Forms.Timer timer;
            private FlowLayoutPanel panel;
            private TaskItem currentTask;
            private Button btnIncreaseTime;
            private Button btnDecreaseTime;
            private Panel innerPanel; // Declare a new Panel

            public TaskControl(TaskItem task)
            {
                this.currentTask = task;
                this.Size = new Size(500, 120); // Set default size for TaskControl
                InitializeTaskControl();
            }

            private Button btnDelete;
            private Button btnPriorityLow;
            private Button btnPriorityMedium;
            private Button btnPriorityHigh;

            private void InitializeTaskControl()
            {
                // Initialize the controls
                this.lblTimeRemaining = new Label();
                this.lblTotalRequired = new Label();
                this.txtTaskName = new TextBox();
                this.btnStart = new Button();
                this.btnStop = new Button();
                this.btnDelete = new Button();
                this.btnPriorityLow = new Button();
                this.btnPriorityMedium = new Button();
                this.btnPriorityHigh = new Button();
                this.timer = new System.Windows.Forms.Timer();
                this.panel = new FlowLayoutPanel();

                this.innerPanel = new Panel();

                // Timer
                timer.Interval = 1000;
                timer.Tick += new EventHandler(Timer_Tick);

                // FlowLayoutPanel
                panel.Dock = DockStyle.Fill;

                // txtTaskName
                txtTaskName.Text = currentTask.Name;
                txtTaskName.Width = this.Width - 15;
                txtTaskName.TextChanged += TxtTaskName_TextChanged; // Add this line

                // lblTotalRequired
                lblTotalRequired.AutoSize = true;
                lblTotalRequired.Text = "Total: " + currentTask.TotalTime.ToString(@"hh\:mm\:ss");


                // lblTimeRemaining
                lblTimeRemaining.AutoSize = true;
                lblTimeRemaining.Text = "Remaining: " + currentTask.TimeRemaining.ToString(@"hh\:mm\:ss");

              

                // Initialize buttons
                btnStart = new Button { Text = "Start", AutoSize = true, BackColor = Color.White };

                // btnIncreaseTime
                btnIncreaseTime = new Button();
                btnIncreaseTime.Text = "+";
                btnIncreaseTime.AutoSize = true;
                btnIncreaseTime.BackColor = Color.White;
                btnIncreaseTime.Click += BtnIncreaseTime_Click;

                // btnDecreaseTime
                btnDecreaseTime = new Button();
                btnDecreaseTime.Text = "-";
                btnDecreaseTime.AutoSize = true;
                btnDecreaseTime.BackColor = Color.White;
                btnDecreaseTime.Click += BtnDecreaseTime_Click;

                btnStop = new Button { Text = "Stop", AutoSize = true, BackColor = Color.White };
                btnDelete = new Button { Text = "Remove", AutoSize = true, BackColor = Color.White };
                btnPriorityLow = new Button { Text = "Low", AutoSize = true, BackColor = Color.Green };
                btnPriorityMedium = new Button { Text = "Medium", AutoSize = true, BackColor = Color.Yellow };
                btnPriorityHigh = new Button { Text = "High", AutoSize = true, BackColor = Color.Red };

                // Attach event handlers
                btnStart.Click += BtnStart_Click;
                btnStop.Click += BtnStop_Click;
                btnDelete.Click += BtnDelete_Click;
                btnPriorityLow.Click += (sender, e) => SetPriority(TaskPriority.Low);
                btnPriorityMedium.Click += (sender, e) => SetPriority(TaskPriority.Medium);
                btnPriorityHigh.Click += (sender, e) => SetPriority(TaskPriority.High);

                // Create and configure the FlowLayoutPanel
                panel.Dock = DockStyle.Fill;
                panel.AutoSize = true;

                // Now add the controls to the panel
                panel.Controls.Add(txtTaskName);
                panel.Controls.Add(lblTimeRemaining);
                panel.Controls.Add(btnIncreaseTime);
                panel.Controls.Add(btnDecreaseTime);
                panel.Controls.Add(lblTotalRequired);
                panel.Controls.Add(btnStart);
                panel.Controls.Add(btnStop);
                panel.Controls.Add(btnDelete);
                panel.Controls.Add(btnPriorityLow);
                panel.Controls.Add(btnPriorityMedium);
                panel.Controls.Add(btnPriorityHigh);


                // Configure the inner panel
                innerPanel.Size = new Size(this.Width - 10, this.Height - 10);
                innerPanel.Location = new Point(5, 5);
                innerPanel.BackColor = Color.LightGray;

                // Add the FlowLayoutPanel to the inner panel
                innerPanel.Controls.Add(panel);

                // Add the inner panel to the TaskControl
                this.Controls.Add(innerPanel);

           
            }


            private void BtnDelete_Click(object sender, EventArgs e)
            {
                Form1 parentForm = this.FindForm() as Form1;
                parentForm?.DeleteTask(this.currentTask);
                parentForm?.UpdateTotalTimeSum();
            }

            public void SetPriority(TaskPriority priority)
            {
                currentTask.Priority = priority;
                // Update UI to reflect the priority
                switch (priority)
                {
                    case TaskPriority.Low:
                        this.BackColor = Color.Green;
                        break;
                    case TaskPriority.Medium:
                        this.BackColor = Color.Yellow;
                        break;
                    case TaskPriority.High:
                        this.BackColor = Color.Red;
                        break;
                }
                TriggerTasksChanged();
                Form1 parentForm = this.FindForm() as Form1;
                parentForm?.SaveTasksToFile();
               
            }

            private void TxtTaskName_TextChanged(object sender, EventArgs e)
            {
                // Update the name of the currentTask when the text changes
                currentTask.Name = txtTaskName.Text;
                Form1 parentForm = this.FindForm() as Form1;
                parentForm?.TasksChanged?.Invoke(); // Invoke the event
            }

            private void BtnStart_Click(object? sender, EventArgs e)
            {
                timer.Start();
            }

            private void BtnStop_Click(object? sender, EventArgs e)
            {
                timer.Stop();
            }



            private void Timer_Tick(object sender, EventArgs e)
            {
                if (currentTask.TimeRemaining.TotalSeconds > 0)
                {
                    // Decrement the time remaining
                    currentTask.TimeRemaining = currentTask.TimeRemaining.Subtract(TimeSpan.FromSeconds(1));
                    lblTimeRemaining.Text = "Remaining: " + currentTask.TimeRemaining.ToString(@"hh\:mm\:ss");
                }
                else
                {
                    timer.Stop();
                    // Optionally notify the user that time has run out for the task
                    // Play a sound to indicate the timer has run out
                    PlaySound();
                }
                // This line ensures the time remaining is saved each second it ticks down
                TriggerTasksChanged();
            }

            private void PlaySound()
            {
                // You can choose from Asterisk, Beep, Exclamation, Hand, or Question.
                SystemSounds.Exclamation.Play();
            }

            private void BtnIncreaseTime_Click(object sender, EventArgs e)
            {
                // Increase the total time required
                currentTask.TotalTime = currentTask.TotalTime.Add(TimeSpan.FromMinutes(15));
                currentTask.TimeRemaining = currentTask.TotalTime; // Reset the TimeRemaining to match
                lblTotalRequired.Text = "Total: " + currentTask.TotalTime.ToString(@"hh\:mm\:ss");
                lblTimeRemaining.Text = "Remaining: " + currentTask.TimeRemaining.ToString(@"hh\:mm\:ss");

                TriggerTasksChanged();
                UpdateTotalTimeSumOnParent(); // Update the total time sum on the parent form
            }

            private void BtnDecreaseTime_Click(object sender, EventArgs e)
            {
                if (currentTask.TotalTime.TotalMinutes > 0)
                {
                    // Decrease the total time required
                    currentTask.TotalTime = currentTask.TotalTime.Subtract(TimeSpan.FromMinutes(15));
                    currentTask.TimeRemaining = currentTask.TotalTime; // Reset the TimeRemaining to match
                    lblTotalRequired.Text = "Total: " + currentTask.TotalTime.ToString(@"hh\:mm\:ss");
                    lblTimeRemaining.Text = "Remaining: " + currentTask.TimeRemaining.ToString(@"hh\:mm\:ss");

                    TriggerTasksChanged();
                    UpdateTotalTimeSumOnParent(); // Update the total time sum on the parent form
                }
            }

            // This method is used to update the total time sum on the parent form
            private void UpdateTotalTimeSumOnParent()
            {
                Form1 parentForm = this.FindForm() as Form1;
                parentForm?.UpdateTotalTimeSum();
            }
            // This method triggers the TasksChanged event on the parent form
            private void TriggerTasksChanged()
            {
                Form1 parentForm = this.FindForm() as Form1;
                parentForm?.TasksChanged?.Invoke();
            }
        }

        public enum TaskPriority
        {
            Low,
            Medium,
            High
        }

        [Serializable]
        public class TaskItem
        {
            public string Name { get; set; }
            public TimeSpan TotalTime { get; set; }
            // Removed [JsonIgnore] so TimeRemaining is saved and loaded.
            public TimeSpan TimeRemaining { get; set; }
            public TaskPriority Priority { get; set; } // Property for priority

            public TaskItem()
            {
                Name = "New Task";
                TotalTime = TimeSpan.Zero;
                TimeRemaining = TotalTime;
                Priority = TaskPriority.Medium; // Default priority
            }

            // No need for a separate ResetTime method since the property is now serialized.
        }

        public class Task
        {
            public string Name { get; set; }
            public TimeSpan TimeRemaining { get; set; }

            public Task()
            {
                this.TimeRemaining = new TimeSpan(0, 0, 0);
            }
        }
    }
}
