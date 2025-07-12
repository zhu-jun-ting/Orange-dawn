using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInstantActivateSlot : CardMaster
{
    // id: 401
    // name: Block Miner
    // desc: Open a linked grid block"

    // id: 402
    // name: Block Miner
    // desc: Open a linked grid block"

    // id: 415
    // name: Block Miner
    // desc: Open a linked grid block"
    
    // id: 416
    // name: Block Miner
    // desc: Open a linked grid block"

    public override void OnCardEnable()
    {
        // Directions: Up, Down, Left, Right
        TryActivateSlot(CardDir.Up, up_link_enabled, up_link_cardmaster);
        TryActivateSlot(CardDir.Down, down_link_enabled, down_link_cardmaster);
        TryActivateSlot(CardDir.Left, left_link_enabled, left_link_cardmaster);
        TryActivateSlot(CardDir.Right, right_link_enabled, right_link_cardmaster);
    }

    private void TryActivateSlot(CardDir dir, bool linkEnabled, CardMaster linkedCard)
    {
        if (!linkEnabled) return;
        if (BoardArea.instance == null) return;
        // Find this card's position in the board
        int myRow = -1, myCol = -1;
        bool found = false;
        for (int r = 0; r < BoardArea.instance.rows; r++)
        {
            for (int c = 0; c < BoardArea.instance.columns; c++)
            {
                if (BoardArea.instance.gridState[r, c] == this)
                {
                    myRow = r;
                    myCol = c;
                    found = true;
                    break;
                }
            }
            if (found) break;
        }
        if (!found) return;
        int targetRow = myRow, targetCol = myCol;
        switch (dir)
        {
            case CardDir.Up: targetRow = myRow - 1; break;
            case CardDir.Down: targetRow = myRow + 1; break;
            case CardDir.Left: targetCol = myCol - 1; break;
            case CardDir.Right: targetCol = myCol + 1; break;
        }
        // Check bounds
        if (targetRow < 0 || targetCol < 0) return;
        // If out of bounds, let ActivateCell handle grid expansion
        bool isOpen = BoardArea.instance.IsCellOpen(targetRow, targetCol);
        if (!isOpen)
        {
            BoardArea.instance.ActivateCell(targetRow, targetCol);
            OnCardDestroyed();
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description));
    }
}
