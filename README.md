# 🌈 Bifrost: Your Job Search Companion

Welcome to **Bifrost**! 👋

Job hunting can feel like a full-time job in itself. Between juggling spreadsheets, sticky notes, and endless email threads, it's easy to lose track of that perfect opportunity.

**Bifrost** is here to bring calm to the chaos. It's a simple, personal tool designed to help you organize your job search journey, so you can focus on what matters most: landing your dream job! 🚀

## ✨ What Can Bifrost Do For You?

* **🎯 Track Opportunities**: Save interesting job postings in one place. No more "Where did I see that link again?"
* **📝 Manage Applications**: Keep tabs on every application. Know exactly when you applied and what stage you're at (Applied, Interviewing, Offer!).
* **💭 Keep Notes**: Store your thoughts, interview feedback, and important details right alongside the job info.
* **⚙️ Set Your Preferences**: Define what you're looking for so you stay focused on your goals.

Think of Bifrost as your personal assistant for your career growth. It remembers the details so you don't have to!

---

## 🛠️ How to Run Locally (For Developers)

If you want to run Bifrost on your own machine, we've made it super easy using Docker.

### Prerequisites

* [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.

### Quick Start

1. **Clone the repository**:

    ```bash
    git clone https://github.com/nusbru/Bifrost.git
    cd Bifrost
    ```

2. **Start the database**:
    We use Docker to run the PostgreSQL database. Run this command to start it:

    ```bash
    docker-compose up -d
    ```

3. **Run the application**:
    Now, start the Bifrost API using the .NET CLI:

    ```bash
    dotnet run --project src/Bifrost.Api
    ```

4. **Access the Application**:
    Once everything is running, you can explore the API documentation here:
    👉 <https://localhost:5037/docs>

Happy job hunting! 🎉
