using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Comments
{
    public partial class CommentsForm : Form
    {
        private Random random = new Random();
        List<User> users = new List<User>();
        List<Comment> comments = new List<Comment>();
        private Button refreshButton;
        private ComboBox sortComboBox;
        private Panel commentsPanel;
        private Image likeImage, dislikeImage;

        public CommentsForm()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.KeyPreview = true;

            likeImage = LoadImage("like.png");
            dislikeImage = LoadImage("dislike.png");

            commentsPanel = new Panel
            {
                Location = new Point(10, 50),
                Size = new Size(520, 500),
                AutoScroll = true
            };

            this.Controls.Add(commentsPanel);

            RefreshComments();
            SortCommentsById();

            refreshButton = new Button { Location = new Point(10, 10), Size = new Size(80, 30), Text = "Refresh", Cursor = Cursors.Hand };
            Label sortLabel = new Label { Location = new Point(100, 15), Text = "Sort by:", AutoSize = true };

            sortComboBox = new ComboBox
            {
                Location = new Point(160, 10),
                Size = new Size(150, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            sortComboBox.Items.AddRange(new string[] { "Default", "Likes", "Dislikes", "Newest", "Oldest" });
            sortComboBox.SelectedIndex = 0;
            sortComboBox.SelectedIndexChanged += SortComboBox_SelectedIndexChanged;

            refreshButton.Click += (sender, e) => RefreshComments();
            this.Controls.Add(refreshButton);
            this.Controls.Add(sortLabel);
            this.Controls.Add(sortComboBox);
            this.KeyDown += CommentsForm_KeyDown;
        }

        private void CommentsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                RefreshComments();
            }
        }

        private void SortComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (sortComboBox.SelectedItem.ToString())
            {
                case "Default":
                    SortCommentsById();
                    break;
                case "Likes":
                    SortCommentsByLikes();
                    break;
                case "Dislikes":
                    SortCommentsByDislikes();
                    break;
                case "Newest":
                    SortCommentsByDateDescending();
                    break;
                case "Oldest":
                    SortCommentsByDateAscending();
                    break;
            }
        }

        private void SortCommentsById()
        {
            comments.Sort((x, y) => x.Id.CompareTo(y.Id));
            DrawComments();
        }

        private void SortCommentsByDateDescending()
        {
            comments.Sort((x, y) => y.Date.CompareTo(x.Date));
            DrawComments();
        }

        private void SortCommentsByDateAscending()
        {
            comments.Sort((x, y) => x.Date.CompareTo(y.Date));
            DrawComments();
        }

        private void DrawComments()
        {
            commentsPanel.SuspendLayout();
            commentsPanel.Controls.Clear();
            int y = 10;

            foreach (Comment comment in comments)
            {
                CommentRow commentRow = new CommentRow(comment, new Point(10, y), likeImage, dislikeImage);
                commentsPanel.Controls.Add(commentRow.GetGroupBox());
                y += 190;
            }

            commentsPanel.ResumeLayout();
        }

        private void GenerateUsers()
        {
            int userCount = random.Next(20, 101);
            for (int i = 0; i < userCount; i++)
            {
                users.Add(new User(Faker.Name.FullName()));
            }
        }

        private void GenerateComments()
        {
            int commentCount = random.Next(20, 26);
            for (int i = 0; i < commentCount; i++)
            {
                string commentText = Faker.Lorem.Sentence(random.Next(5, 15));
                int userIndex = random.Next(0, users.Count);
                DateTime commentDate = RandomDay();
                comments.Add(new Comment(commentText, users[userIndex], commentDate, random.Next(1, 500), random.Next(1, 100)));
            }
        }

        private void RefreshComments()
        {
            users.Clear();
            comments.Clear();

            GenerateUsers();
            GenerateComments();
            SortCommentsById();
        }

        private void SortCommentsByLikes()
        {
            comments.Sort((x, y) => y.Likes.CompareTo(x.Likes));
            DrawComments();
        }

        private void SortCommentsByDislikes()
        {
            comments.Sort((x, y) => y.Dislikes.CompareTo(x.Dislikes));
            DrawComments();
        }

        private DateTime RandomDay()
        {
            DateTime start = new DateTime(2020, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(random.Next(range));
        }

        private Image LoadImage(string imageName)
        {
            string imagePath = Path.Combine(Application.StartupPath, "Assets", imageName);
            if (File.Exists(imagePath))
            {
                using (Image originalImage = Image.FromFile(imagePath))
                {
                    Bitmap resizedImage = new Bitmap(originalImage, new Size(40, 40));
                    return resizedImage;
                }
            }
            else
            {
                MessageBox.Show($"Image not found", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }

    public class User
    {
        private static int autoInc = 1;
        public string Name { get; set; }
        public int Id { get; }

        public User(string name)
        {
            Name = name;
            Id = autoInc++;
        }
    }

    public class Comment
    {
        private static int autoInc = 1;
        public int Id { get; }
        public string Body { get; set; }
        public User CommentUser { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public DateTime Date { get; set; }

        public Comment(string body, User user, DateTime date, int likes = 0, int dislikes = 0)
        {
            Id = autoInc++;
            Body = body;
            CommentUser = user;
            Likes = likes;
            Dislikes = dislikes;
            Date = date;
        }
    }

    public class CommentRow
    {
        private Comment comment;
        private Label likesLabel, dislikesLabel, dateLabel;
        private Button likeButton, dislikeButton;
        private bool hasLiked = false;
        private bool hasDisliked = false;
        GroupBox commentRow;

        public CommentRow(Comment comment, Point position, Image likeImage, Image dislikeImage)
        {
            this.comment = comment;
            commentRow = new GroupBox
            {
                Location = position,
                Size = new Size(480, 180),
                Text = $"Comment #{comment.Id}"
            };

            Label userNameLabel = new Label { Text = comment.CommentUser.Name, Location = new Point(10, 20) };
            Label commentBodyLabel = new Label { Text = comment.Body, Location = new Point(10, 50) };
            dateLabel = new Label { Text = $"Date: {comment.Date.ToShortDateString()}", Location = new Point(370, 15), Width = 100, TextAlign = ContentAlignment.MiddleRight };
            likesLabel = new Label { Text = $"Likes: {comment.Likes}", Location = new Point(10, 80), Width = 80 };
            dislikesLabel = new Label { Text = $"Dislikes: {comment.Dislikes}", Location = new Point(100, 80), Width = 80 };

            likeButton = new Button
            {
                Location = new Point(10, 110),
                Size = new Size(60, 60),
                Image = likeImage,
                ImageAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };

            likeButton.Click += (sender, e) =>
            {
                if (hasLiked)
                {
                    comment.Likes--;
                    hasLiked = false;
                    likeButton.BackColor = SystemColors.Control;
                }
                else
                {
                    comment.Likes++;
                    hasLiked = true;
                    likeButton.BackColor = Color.LightGray;

                    if (hasDisliked)
                    {
                        comment.Dislikes--;
                        hasDisliked = false;
                        dislikeButton.BackColor = SystemColors.Control;
                    }
                }
                likesLabel.Text = $"Likes: {comment.Likes}";
                dislikesLabel.Text = $"Dislikes: {comment.Dislikes}";
            };

            dislikeButton = new Button
            {
                Location = new Point(100, 110),
                Size = new Size(60, 60),
                Image = dislikeImage,
                ImageAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };

            dislikeButton.Click += (sender, e) =>
            {
                if (hasDisliked)
                {
                    comment.Dislikes--;
                    hasDisliked = false;
                    dislikeButton.BackColor = SystemColors.Control;
                }
                else
                {
                    comment.Dislikes++;
                    hasDisliked = true;
                    dislikeButton.BackColor = Color.LightGray;

                    if (hasLiked)
                    {
                        comment.Likes--;
                        hasLiked = false;
                        likeButton.BackColor = SystemColors.Control;
                    }
                }
                dislikesLabel.Text = $"Dislikes: {comment.Dislikes}";
                likesLabel.Text = $"Likes: {comment.Likes}";
            };

            commentRow.Controls.Add(userNameLabel);
            commentRow.Controls.Add(commentBodyLabel);
            commentRow.Controls.Add(dateLabel);
            commentRow.Controls.Add(likesLabel);
            commentRow.Controls.Add(dislikesLabel);
            commentRow.Controls.Add(likeButton);
            commentRow.Controls.Add(dislikeButton);
        }

        public GroupBox GetGroupBox()
        {
            return commentRow;
        }
    }


}
