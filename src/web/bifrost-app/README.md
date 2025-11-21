# 🌉 Bifrost - Job Application Tracker
## 🛠️ How to Run the Project

### Prerequisites

Before you start, make sure you have:
- A computer with internet connection
- [Node.js](https://nodejs.org/) installed (version 20 or higher)
- A [Supabase](https://supabase.com) account (free to create)

### Option 1: Run Locally (For Development)

**Step 1: Get Your Supabase Credentials**

1. Go to [supabase.com](https://supabase.com) and sign in
2. Create a new project or select an existing one
3. Go to Project Settings → API
4. Copy your **Project URL** and **Anon/Public Key**

**Step 2: Set Up Your Environment**

1. Open the project folder in your terminal
2. Navigate to the application directory:
   ```bash
   cd src/web/bifrost-app
   ```

3. Create a file named `.env` and add your Supabase credentials:
   ```env
   NEXT_PUBLIC_SUPABASE_URL=your_project_url_here
   NEXT_PUBLIC_SUPABASE_ANON_KEY=your_anon_key_here
   NEXT_PUBLIC_API_BASE_URL=https://localhost:7001
   ```

**Step 3: Install and Run**

1. Install the required packages:
   ```bash
   npm install
   ```

2. Start the application:
   ```bash
   npm run dev
   ```

3. Open your web browser and go to: `http://localhost:3000`

🎉 **That's it!** Bifrost should now be running on your computer.

### Option 2: Run with Docker (For Production)

Docker makes it easy to run Bifrost in a containerized environment.

**Step 1: Build the Docker Image**

```bash
cd src/web/bifrost-app
docker build -t bifrost-app:latest .
```

**Step 2: Run the Container**

Replace the placeholder values with your actual credentials:

```bash
docker run -p 3000:3000 \
  -e NEXT_PUBLIC_SUPABASE_URL=your_supabase_url \
  -e NEXT_PUBLIC_SUPABASE_ANON_KEY=your_supabase_key \
  -e NEXT_PUBLIC_API_BASE_URL=https://localhost:7001 \
  bifrost-app:latest
```

**Step 3: Access the Application**

Open your browser and visit: `http://localhost:3000`

### Running Tests

To make sure everything is working correctly:

```bash
npm test
```

All tests should pass with a green checkmark ✅

### Stopping the Application

**For Local Development:**
- Press `Ctrl + C` in your terminal

**For Docker:**
```bash
docker ps  # Find the container ID
docker stop <container_id>
```

### Common Issues & Solutions

**Issue**: "Port 3000 is already in use"
- **Solution**: Either stop the other application using port 3000, or run Bifrost on a different port:
  ```bash
  PORT=3001 npm run dev
  ```

**Issue**: "Cannot connect to Supabase"
- **Solution**: Double-check your Supabase URL and key in the `.env` file

**Issue**: "Module not found" errors
- **Solution**: Delete the `node_modules` folder and run `npm install` again

### Need Help?

If you run into any problems:
1. Check that all prerequisites are installed
2. Verify your environment variables are correct
3. Make sure you're in the correct directory
4. Try restarting the application

---

## 📚 Additional Resources

- [Supabase Documentation](https://supabase.com/docs)
- [Next.js Documentation](https://nextjs.org/docs)
- [Docker Documentation](https://docs.docker.com/)

---

**Built with ❤️ to make job searching easier**
