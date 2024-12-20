// SPDX-License-Identifier: MIT
pragma solidity ^0.8.17;

// Define the "ColorfulChaos" contract
contract ColorfulChaos {
    // Use a mapping to store the number of chaos points each player owns.
    mapping(address => uint256) private chaosPoints;

    // Events to record changes on the blockchain:
    event ChaosPointsUpdated(address indexed player, uint256 totalPoints);

    // Function to retrieve a player's chaos points.
    function getChaosPoints(address player) external view returns (uint256) {
        return chaosPoints[player];
    }

    // Function to increment chaos points for a player by 1.
    function incrementChaosPoints(address player) external {
        chaosPoints[player] += 1;
        emit ChaosPointsUpdated(player, chaosPoints[player]);
    }

    // Function to reset a player's chaos points to zero.
    function resetChaosPoints(address player) external {
        chaosPoints[player] = 0;
        emit ChaosPointsUpdated(player, 0);
    }
}
