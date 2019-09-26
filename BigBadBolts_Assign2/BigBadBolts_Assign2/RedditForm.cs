﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace BigBadBolts_Assign2
{


    public partial class RedditForm : Form
    {
        static public Nullable<uint> currentUserID;
        static public bool superuser = false;
        static public bool moderator = false;
        static public SortedSet<Post> myPosts = new SortedSet<Post>();
        static public SortedSet<Comment> myComments = new SortedSet<Comment>();
        static public SortedSet<Subreddit> mySubReddits = new SortedSet<Subreddit>();
        static public SortedSet<User> myUsers = new SortedSet<User>();


        public RedditForm()
        {
            //Read the input files here to build the objects
            HelperFunctions.getFileInput(myPosts, myComments, mySubReddits, myUsers);
            InitializeComponent(); //This needs to be towards the top of the program!!!

            LoadTables();
            ToggleSubLabels(false);
        }

        private void LoadTables()
        {
            foreach (User user in myUsers) //populate the user listBox
            {
                if (user.Type == 1)//This is the moderator
                {
                    userListBox.Items.Add(user.ToString() + "   [M]");
                }
                else if (user.Type == 2)//This it the super user
                {
                    userListBox.Items.Add(user.ToString() + "   [A]");
                }
                else
                {
                    userListBox.Items.Add(user.ToString());
                }
            }
            foreach (Subreddit sub in mySubReddits) //populate the subreddit listBox
            {
                subredditListBox.Items.Add(sub.ToString());
            }
        }

        private void LoginBtn_Click(object sender, EventArgs e)
        {

            if (userListBox.SelectedIndex == -1)//check to make sure an option was selected
            {
                systemOutListBox.Items.Add("Please select a user to login as.");
                return;
            }

            Button login = sender as Button;

            if (login.Text == "Login") //this is to login
            {
                foreach (User user in myUsers)
                {
                    //Need to fix it so these lines are not needed aka compare by user id
                    string curUser = (string)userListBox.SelectedItem;
                    curUser = curUser.Split(' ')[0];
                    if ((string)userListBox.SelectedItem == user.Name || curUser == user.Name)//need to remove this second part
                    {
             

                        string selectedName = userListBox.SelectedItem.ToString();
                        selectedName = selectedName.Split(' ')[0];

                    
                        //converts the selected user name to a string
                        bool loginSuccess = false; //used to prompt password is correct or not

                        string hashCode = user.PasswordHash.ToString(); //the hashpassword from the user.txt file
                        string inputPassword = passwordTextBox.Text.GetHashCode().ToString("X"); //the hash password that the user inputs

                        if (hashCode == inputPassword)
                        {
                            loginSuccess = true;
                        }

                        if(hashCode != inputPassword)
                        {
                            loginSuccess = false;
                        }

                        if (loginSuccess) //if the password was correct, log in
                        {
                            if (user.Type == 2)//Super user, must implement enumeration on this
                            {
                                superuser = true;
                            }
                            if (user.Type == 1)//Moderator, must implement enumeration on this
                            {
                                moderator = true;
                            }

                            currentUserID = user.Id;
                            postListBox.Items.Clear();
                            commentListBox.Items.Clear();
                            foreach (Post userPost in myPosts)
                            {
                                if (userPost.PostAuthorId == currentUserID)
                                {
                                    postListBox.Items.Add(userPost.ToString());
                                }
                                
                            }
                            if(postListBox.Items.Count == 0)
                            {
                                postListBox.Items.Add("There are no posts by this user.");
                                postListBox.Enabled = false;
                            }
                            else
                            {
                                postListBox.Enabled = true;
                            }
                            foreach (Comment userComment in myComments)
                            {
                                if (userComment.CommentAuthorId == currentUserID)
                                {
                                    commentListBox.Items.Add(userComment.ToString());
                                }
                            }
                            if (commentListBox.Items.Count == 0)
                            {
                                commentListBox.Items.Add("There are no comments by this user.");
                                commentListBox.Enabled = false;
                            }
                            else
                            {
                                commentListBox.Enabled = true;
                            }
                            systemOutListBox.Items.Add("We are logged in as user: " + userListBox.SelectedItem);
                            systemOutListBox.Items.Add("Getting all posts and comments by " + userListBox.SelectedItem);
                            login.Text = "Logout";
                            userListBox.Enabled = false;
                        }

                        if (!loginSuccess) //login not a success, prompt to try again
                        {
                            systemOutListBox.Items.Add("The password is not right, please try again.");
                            userListBox.Enabled = true;
                        }

                        passwordTextBox.Text = String.Empty; //clears the password textbox
                        break;
                    }
                }
            }
            else //This is to log out
            {
                systemOutListBox.Items.Add("Goodbye " + userListBox.SelectedItem);
                currentUserID = null;
                superuser = false;
                moderator = false;
                lockPostBtn.Visible = false;
                userListBox.Enabled = true;
                login.Text = "Login";
                postListBox.Items.Clear();
                commentListBox.Items.Clear();
                addReplyBtn.Enabled = false;
                addReplyTextBox.Text = "";
                addReplyTextBox.Enabled = false;
                ToggleSubLabels(false);
                subredditListBox.ClearSelected();
                deleteCommentBtn.Enabled = false;
                deletePostBtn.Enabled = false;
            }
        }

        private void ToggleSubLabels(bool status)
        {
            membersNumberLabel.Visible = status;
            membersTileLabel.Visible = status;
            activeNumberLabel.Visible = status;
            activeTitleLabel.Visible = status;
        }

        public void LoadPosts()
        {
            postListBox.Enabled = true;
            postListBox.Items.Clear();//clear out anything that might have been in it before
            uint subIDToView = 0;
            if ((string)subredditListBox.SelectedItem == "all") //determines if all the posts should be displayed
            {
                ToggleSubLabels(false);
                foreach (Post post in myPosts)//display all the posts
                {
                    postListBox.Items.Add(post.ToString());
                }
                systemOutListBox.Items.Add("We are getting all the posts");
            }
            else //only a single subbreddits post should be displayed
            {
                foreach (Subreddit sub in mySubReddits) //Find the id of the subbreddit selected
                {
                    if ((string)subredditListBox.SelectedItem == sub.Name) //Found the subbreddit
                    {
                        subIDToView = sub.Id;

                        ToggleSubLabels(true);
                        membersNumberLabel.Text = sub.Members.ToString();
                        activeNumberLabel.Text = sub.Active.ToString();

                        break;
                    }
                }
                //systemOutListBox.Items.Add("We are getting the posts for subbreddit '" + subIDToView + "'");


                foreach (Post subPost in myPosts) // Display all the posts that have the subReddit as its parent
                {
                    if (subIDToView == subPost.SubHome)
                    {
                        postListBox.Items.Add(subPost.ToString());
                    }
                }
            }
            if (postListBox.Items.Count == 0)
            {
                postListBox.Items.Add("There are no posts for this Subreddit.");
                postListBox.Enabled = false;
            }
        }

        public void LoadComments()
        {
            commentListBox.Enabled = true;
            commentListBox.Items.Clear();//clear out anything that might have been in it before
            uint postIDToView = UInt32.Parse(HelperFunctions.getBetween((string)postListBox.SelectedItem, "<", ">"));

            foreach (Comment comment in myComments) //Find the comments to the post
            {
                if (postIDToView == comment.ParentID) //Found a comment to the post
                {
                    commentListBox.Items.Add(comment.ToString());
                    foreach (Comment reply in myComments)
                    {
                        if (reply.ParentID == comment.CommentID)
                        {
                            commentListBox.Items.Add('\t' + reply.ToString());
                            foreach (Comment AnotherReply in myComments)
                            {
                                if (AnotherReply.ParentID == reply.CommentID)
                                {
                                    commentListBox.Items.Add("\t\t" + AnotherReply.ToString());
                                }
                            }
                        }
                    }

                }
            }
            if (commentListBox.Items.Count == 0)
            {
                commentListBox.Items.Add("There are no comments to view.");
                commentListBox.Enabled = false;
            }
        }

        private void SubredditListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            deleteCommentBtn.Enabled = false;
            deletePostBtn.Enabled = false;
            commentListBox.Items.Clear();
            addReplyTextBox.Text = "";
            addReplyTextBox.Enabled = false;
            addReplyBtn.Enabled = false;
            if (subredditListBox.SelectedIndex == -1)//nothing is selected in the subreddits list box
            {
                ToggleSubLabels(false);
            }

            LoadPosts();
            if( moderator || superuser)//If the user is currently a super or moderator
            {
                lockPostBtn.Visible = true;
                lockPostBtn.Enabled = true;
            }
        }

        private void PostListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            deleteCommentBtn.Enabled = false;
            addReplyTextBox.Text = "";
            deletePostBtn.Enabled = true;
            addReplyTextBox.Enabled = true;
            addReplyBtn.Enabled = true;
            foreach (Post post in myPosts)
            {
                if (post.PostID == UInt32.Parse(HelperFunctions.getBetween(postListBox.SelectedItem.ToString(), "<", ">")))
                {
                    if (post.Locked == true)
                    {
                        if( superuser || moderator)
                        {
                            lockPostBtn.Text = "Unlock Post";
                        }
                        addReplyBtn.Enabled = false;
                        addReplyTextBox.Text = "This post is locked and cannot be edited.";
                        addReplyTextBox.Enabled = false;
                    }
                    else if (post.Locked == false)
                    {
                        if( superuser || moderator)
                        {
                            lockPostBtn.Text = "Lock Post";
                        }
                    }
                }
            }
            
            LoadComments();
           // systemOutListBox.Items.Add("We are getting the comment for post '" + UInt32.Parse(HelperFunctions.getBetween((string)postListBox.SelectedItem, "<", ">")) + "'");
        }

        private void AddReplyBtn_Click(object sender, EventArgs e)
        {
            string content = "";
            uint ID = 0;
            if (currentUserID == null)//Make sure user logged in
            {
                MessageBox.Show("Please log in to comment.");
                return;
            }
            if (addReplyTextBox.TextLength <= 0) //make sure words are present
            {
                MessageBox.Show("Please enter in a comment to add.");
                return;
            }



            //SHOULD BE GOOD TO SAVE THE COMMENT

            // systemOutListBox.Items.Add("This is the index of selected item: " + commentListBox.SelectedIndex);
            if (commentListBox.SelectedIndex != -1)//make sure the comment has something listed
            {
                uint commentToReplyID;
                try
                {
                    commentToReplyID = UInt32.Parse(HelperFunctions.getBetween(commentListBox.SelectedItem.ToString(), "<", ">"));
                }
                catch (Exception ex)
                {
                    commentToReplyID = UInt32.Parse(HelperFunctions.getBetween(postListBox.SelectedItem.ToString(), "<", ">"));

                }
                systemOutListBox.Items.Add(commentToReplyID.ToString());

                foreach (Comment commentToReply in myComments) //Search for the comment to reply to
                {
                    if (commentToReply.CommentID == commentToReplyID)//Found the comment to reply to
                    {
                        string commentContent = addReplyTextBox.Text;
                        try
                        {
                            if (HelperFunctions.vulgarityChecker(commentContent))
                            {
                                throw new HelperFunctions.FoulLanguageException();
                            }
                        }
                        catch (HelperFunctions.FoulLanguageException fle)
                        {

                            MessageBox.Show(fle.ToString(), "BAD WORD DETECTED");
                            return;
                        }
                        content = commentContent;
                        ID = commentToReply.CommentID;
                        addReplyTextBox.Text = "";
                    }
                }
            }
            else // Not selected comment, add to post
            {
                foreach (Post post in myPosts)
                {
                    if (post.PostID == UInt32.Parse(HelperFunctions.getBetween(postListBox.SelectedItem.ToString(), "<", ">")))
                    {
                        string commentContent = addReplyTextBox.Text;
                        try
                        {
                            if (HelperFunctions.vulgarityChecker(commentContent))
                            {
                                throw new HelperFunctions.FoulLanguageException();
                            }
                        }
                        catch (HelperFunctions.FoulLanguageException fle)
                        {

                            MessageBox.Show(fle.ToString(), "BAD WORD DETECTED");
                            return;
                        }
                        content = commentContent;
                        ID = post.PostID;
                        addReplyTextBox.Text = "";
                        break;
                    }
                }


            }
            Comment replyToAdd = new Comment(
                        content, //content
                        (uint)currentUserID, //authorID 
                        ID //parentID
                    );




            if (myComments.Add(replyToAdd))
                systemOutListBox.Items.Add("Succesfully Added a new comment.");
            else
                systemOutListBox.Items.Add("We could not add your comment. Try again later.");

            LoadComments();
        }

        private void DeletePostBtn_Click(object sender, EventArgs e)
        {
            if (postListBox.SelectedIndex == -1)//Nothing is selected to delete
            {
                MessageBox.Show("Please select a post to delete.");
                return;
            }
            foreach (Post post in myPosts)//search through the posts to find the one to delete.
            {
                if (post.PostID == UInt32.Parse(HelperFunctions.getBetween(postListBox.SelectedItem.ToString(), "<", ">")))//found the selected post
                {
                    if (post.PostAuthorId == currentUserID || superuser) //the correct logged in user is trying to delete
                    {
                        if (myPosts.Remove(post))//check to make sure it deleted correctly
                        {
                            deleteCommentBtn.Enabled = false;
                            commentListBox.Items.Clear();
                            addReplyTextBox.Enabled = false;
                            addReplyBtn.Enabled = false;
                            LoadPosts();//refresh the data in the table
                            systemOutListBox.Items.Add("Successfully deleted the post.");
                            return;
                        }
                        else
                            systemOutListBox.Items.Add("Tried to delete, but something went wrong.");

                    }
                    else //either no user or access not allowed
                    {
                        MessageBox.Show("You do not have permission to delete this post");
                    }
                    break;
                }
            }
        }

        private void DeleteCommentBtn_Click(object sender, EventArgs e)
        {
            if (commentListBox.SelectedIndex == -1)//nothing is selected
            {
                MessageBox.Show("Please select a comment to delete.");
                return;
            }
            foreach (Comment comment in myComments)//search through the comments to find the one to delete.
            {
                if (comment.CommentID == UInt32.Parse(HelperFunctions.getBetween(commentListBox.SelectedItem.ToString(), "<", ">")))//found the selected comment
                {
                    if (comment.CommentAuthorId == currentUserID || superuser) //the correct logged in user is trying to delete
                    {
                        if (myComments.Remove(comment))//check to make sure it deleted correctly
                        {

                            LoadComments();//refresh the data in the table
                            systemOutListBox.Items.Add("Successfully deleted the comment.");
                            return;
                        }
                        else
                            systemOutListBox.Items.Add("Tried to delete, but something went wrong.");

                    }
                    else //either no user or access not allowed
                    {
                        MessageBox.Show("You do not have permission to delete this post");
                    }
                    break;
                }
            }
        }

        private void CommentListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            deleteCommentBtn.Enabled = true;
        }

        private void UserListBox_Click(object sender, EventArgs e) //used to prompt the user to enter the password
        {
            string selectedName = userListBox.SelectedItem.ToString();
            systemOutListBox.Items.Add("Please provide the password for: " + selectedName);
        }

        private void LockPostBtn_Click(object sender, EventArgs e)
        {
            if (postListBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a post to lock.");
                return;
            }
            foreach (Post post in myPosts)//search through the posts to find the one to lock.
            {
                if (post.PostID == UInt32.Parse(HelperFunctions.getBetween(postListBox.SelectedItem.ToString(), "<", ">")))//found the selected post
                {
                    if (moderator || superuser) //the correct logged in user is trying to lock
                    {
                        if (post.Locked == true)
                        {
                            int chosen = postListBox.SelectedIndex;
                            lockPostBtn.Text = "Unlock Post";
                            post.Locked = false;
                            LoadPosts();
                            postListBox.SelectedIndex = chosen;
                        }
                        else
                        {
                            int chosen = postListBox.SelectedIndex;
                            lockPostBtn.Text = "Lock Post";
                            post.Locked = true;
                            LoadPosts();
                            postListBox.SelectedIndex = chosen;
                        }
                        break;
                    }
                }

            }
        }


    }
}