# 🌈 Bifrost: Your Job Search Companion

Welcome to **Bifrost**! 👋

Job hunting can feel overwhelming. Between juggling spreadsheets, sticky notes, and endless email threads, it's easy to lose track of opportunities or forget important details from that interview last week.

**Bifrost** is here to bring order to the chaos. Think of it as your personal assistant for job hunting – it organizes everything in one place so you can focus on what really matters: landing your next great opportunity! 🚀

## ✨ What Makes Bifrost Special?

Bifrost helps you stay organized throughout your entire job search journey:

* **🎯 Save Jobs You Love**: Bookmark interesting positions with all the details – company name, job title, location, and the link to apply. Never lose track of "that perfect job" again!

* **📊 Track Your Applications**: Watch your progress unfold. See at a glance which applications are pending, which ones led to interviews, and celebrate those offers!

* **💭 Capture Your Thoughts**: Add notes after phone screens, jot down interview questions you were asked, or remember that great question you want to ask the hiring manager.

* **⚙️ Set Your Goals**: Define what you're looking for (remote work? relocation support? salary range?) and keep your search focused on what matters to you.

* **🔒 Keep It Private**: Your data stays yours. Bifrost is designed with privacy in mind – your job search is personal, and we keep it that way.

Think of Bifrost as the notebook you wish you had for your last job search, but better organized and always available! 📓✨

---

## 🚀 Getting Started

Ready to take control of your job search? Here's how to get Bifrost up and running!

### What You'll Need

* **Docker Desktop** – This helps run Bifrost on your computer. [Download it here](https://www.docker.com/products/docker-desktop/) (it's free!)
* **A Supabase account** – Used for secure authentication. [Sign up here](https://supabase.com) (also free!)
* **A few minutes** – We've made this as simple as possible!

### Let's Get You Set Up

#### Step 1: Download Bifrost

```bash
git clone https://github.com/nusbru/Bifrost.git
cd Bifrost
```

#### Step 2: Set Up Your Supabase Account

Bifrost uses Supabase for secure user authentication. Here's how to get your credentials:

1. Go to [supabase.com](https://supabase.com) and sign in (or create a free account)
2. Create a new project (give it any name you like!)
3. Once your project is ready, go to **Project Settings** → **API**
4. You'll see two important values:
   * **Project URL** (looks like `https://xxxxx.supabase.co`)
   * **anon/public key** (a long string of characters)

#### Step 3: Configure Your Environment

Now let's tell Bifrost about your Supabase project:

1. In the Bifrost folder, make a copy of the example environment file:

   ```bash
   cp .env.example .env
   ```

2. Open the `.env` file in any text editor
3. Replace the placeholder values with your Supabase credentials:

   ```env
   NEXT_PUBLIC_SUPABASE_URL=https://your-project-id.supabase.co
   NEXT_PUBLIC_SUPABASE_PUBLISHABLE_KEY=your-supabase-anon-key-here
   ```

   *(Paste your actual Project URL and anon key from Step 2)*

💡 **Tip:** These credentials are safe to use – they're public keys designed for client-side applications!

#### Step 4: Start Everything with One Command

We've packaged everything you need into Docker containers. Just run:

```bash
docker-compose up -d
```

This command starts:

* 🗄️ Your personal database (where your job info is stored)
* 🌐 The Bifrost application
* 💻 The web interface

#### Step 5: Open Bifrost in Your Browser

Once everything starts (it takes about 30 seconds), visit:

👉 **<http://localhost:3000>** – Your Bifrost dashboard

You'll be greeted with a sign-in page. Create your account, and you're ready to go!

Want to explore the technical side? Check out the API documentation at:

👉 **<http://localhost:5037/docs>** – Interactive API explorer

### That's It! 🎉

You're ready to start organizing your job search. Create your account, add your first job posting, and see how much easier this makes everything!

---

## 🌟 License

Bifrost is open source and available under the MIT License. Feel free to use it, modify it, and make it your own!

---

**Happy job hunting! May your pipeline be full and your offers be plenty!** 🎯✨
