const express = require("express");
const router = express.Router();
const bcrypt = require("bcrypt");
const jwt = require("jsonwebtoken");

const User = require("../models/User");
const Progress = require("../models/Progress");

// REGISTER
router.post("/register", async (req, res) => {
    const { email, password } = req.body;

    const exists = await User.findOne({ email });
    if (exists) return res.status(400).json({ message: "Email đã tồn tại" });

    const passwordHash = await bcrypt.hash(password, 10);

    const user = await User.create({ email, passwordHash });

    await Progress.create({
        userId: user._id,
        exp: 0,
        gold: 0,
        inventory: []
    });

    res.json({ message: "Đăng ký thành công" });
});

// LOGIN
router.post("/login", async (req, res) => {
    const { email, password } = req.body;

    const user = await User.findOne({ email });
    if (!user) return res.status(400).json({ message: "Email không tồn tại" });

    const valid = await bcrypt.compare(password, user.passwordHash);
    if (!valid) return res.status(400).json({ message: "Sai mật khẩu" });

    const token = jwt.sign({ id: user._id }, "SECRET_KEY", { expiresIn: "7d" });

    res.json({ token });
});

module.exports = router;
