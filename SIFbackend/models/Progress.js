const mongoose = require("mongoose");

const ProgressSchema = new mongoose.Schema({
    userId: { type: mongoose.Schema.Types.ObjectId, ref: "User" },
    exp: { type: Number, default: 0 },
    gold: { type: Number, default: 0 },
    inventory: [
        {
            itemId: String,
            quantity: Number
        }
    ],
    updatedAt: { type: Date, default: Date.now }
});

module.exports = mongoose.model("Progress", ProgressSchema);
