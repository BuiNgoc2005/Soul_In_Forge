
const express = require("express");
const cors = require("cors");
const connectDB = require("./config/db");

const app = express();
app.use(cors());
app.use(express.json());

// Kết nối MongoDB
connectDB();

app.get("/", (req, res) => {
    res.send("Backend is running!");
});

// Routes
app.use("/auth", require("./routes/auth"));
app.use("/user", require("./routes/user"));

app.listen(3000, () => console.log("Server chạy tại cổng 3000"));
