const express = require("express");
const router = express.Router();
const auth = require("../middleware/auth");
const Progress = require("../models/Progress");

// GET PROGRESS
router.get("/profile", auth, async (req, res) => {
    const data = await Progress.findOne({ userId: req.user.id });
    res.json(data);
});

// SAVE PROGRESS
router.post("/save", auth, async (req, res) => {
    const { exp, gold, inventory } = req.body;

    await Progress.findOneAndUpdate(
        { userId: req.user.id },
        { exp, gold, inventory, updatedAt: Date.now() }
    );

    res.json({ message: "Đã lưu tiến trình" });
});

module.exports = router;
