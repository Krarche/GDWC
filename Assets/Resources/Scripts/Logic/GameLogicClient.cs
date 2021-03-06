﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using Network;
using Tools;

namespace Logic {

    public class GameLogicClient : GameLogic {

        public static GameLogicClient game;
        public Game1v1 game1v1;

        public Player localPlayer;
        public Entity localEntity {
            get { return localPlayer.playerEntity; }
        }
        public Queue<Data.Action> localActions {
            get { return localEntity.actions; }
        }
        public SpellInstance[] localSpells {
            get {
                return localEntity.spells;
            }
        }

        public GameLogicClient(string mapId) : base(mapId) {
            localClock = ClockMaster.clientSingleton;
            game = this;
        }

        public void buttonInput(ButtonType type) {
            Debug.Log(type);
            switch (type) {
                case ButtonType.ACTION_ROOT:
                    buttonRootHandler();
                    break;
                case ButtonType.ACTION_MOVEMENT:
                    buttonMovementHandler();
                    break;
                case ButtonType.ACTION_QUICK_SPELL:
                    buttonQuickSpellHandler();
                    break;
                case ButtonType.ACTION_SLOW_SPELL:
                    buttonSlowSpellHandler();
                    break;
                case ButtonType.SPELL_0:
                    buttonSpellHandler((short)(type - ButtonType.SPELL_0));
                    break;
                case ButtonType.SPELL_1:
                    buttonSpellHandler((short)(type - ButtonType.SPELL_0));
                    break;
                case ButtonType.SPELL_2:
                    buttonSpellHandler((short)(type - ButtonType.SPELL_0));
                    break;
                case ButtonType.SPELL_3:
                    buttonSpellHandler((short)(type - ButtonType.SPELL_0));
                    break;
                case ButtonType.CONFIRM:
                    buttonConfirmHandler();
                    break;
                case ButtonType.CANCEL:
                    buttonCancelHandler();
                    break;
                case ButtonType.READY:
                    buttonReadyHandler();
                    break;
            }
        }

        #region Action selection State

        public static short ACTION_SELECTION_STATE_ROOT = 0;
        public static short ACTION_SELECTION_STATE_QUICK = 1;
        public static short ACTION_SELECTION_STATE_SLOW = 2;
        public static short ACTION_SELECTION_STATE_MOVEMENT = 3;
        public static short ACTION_SELECTION_STATE_READY = 4;

        #endregion

        #region Selected spells index

        public static short SELECTED_SPELL_NONE = -1;
        public static short SELECTED_SPELL_1 = 0;
        public static short SELECTED_SPELL_2 = 1;
        public static short SELECTED_SPELL_3 = 2;
        public static short SELECTED_SPELL_4 = 3;

        #endregion

        #region CONTROl_VIEW

        public short currentActionSelectionState = 0;

        public bool canQuickSpell;
        public bool canSlowSpell;
        public bool canMove;

        public bool isAiming {
            get {
                return isMoving || isSpelling;
            }
        }
        public bool isSpelling {
            get {
                return isQuickSpelling || isSlowSpelling;
            }
        }
        public bool isQuickSpelling {
            get {
                return currentActionSelectionState == ACTION_SELECTION_STATE_QUICK;
            }
        }
        public bool isSlowSpelling {
            get {
                return currentActionSelectionState == ACTION_SELECTION_STATE_SLOW;
            }
        }
        public bool isMoving {
            get {
                return currentActionSelectionState == ACTION_SELECTION_STATE_MOVEMENT;
            }
        }

        public short currentSelectedSpell = -1;
        public bool isAnySpellSelected {
            get {
                return currentSelectedSpell != SELECTED_SPELL_NONE;
            }
        }
        public SpellInstance selectedSpell {
            get {
                if (currentSelectedSpell != SELECTED_SPELL_NONE) {
                    return localSpells[currentSelectedSpell];
                }
                return null;
            }
        }

        private Cell currentTargetCell = null;
        List<Cell> movementRangeCells = null;
        List<Cell> movementPathCells = null;
        List<Cell> spellRangeCells = null;
        List<Cell> effectRangeCells = null;
        private Data.Action currentAction = null;

        private void buttonRootHandler() {
            if (currentActionSelectionState != ACTION_SELECTION_STATE_ROOT) {
                if (isSpelling)
                    clearSpellRangeCells();
                else if (isMoving)
                    clearMovementRangeCells();
                currentActionSelectionState = ACTION_SELECTION_STATE_ROOT;
            }
        }

        private void buttonMovementHandler() {
            if (currentActionSelectionState != ACTION_SELECTION_STATE_MOVEMENT) {
                if (isSpelling)
                    clearSpellRangeCells();
                else if (isMoving)
                    clearMovementRangeCells();

                movementRangeCells = grid.getCellsInRange(localEntity.getPriorityCurrentCell(), Math.Min(1, localEntity.currentMP), Math.Min(3, localEntity.currentMP), SpellData.RANGE_AREA_CIRCLE);
                grid.SetCellColor(movementRangeCells, Color.green);

                currentActionSelectionState = ACTION_SELECTION_STATE_MOVEMENT;
            }
        }
        private void buttonQuickSpellHandler() {
            if (currentActionSelectionState != ACTION_SELECTION_STATE_QUICK) {
                if (isMoving)
                    clearMovementRangeCells();
                else if (isSpelling)
                    clearSpellRangeCells();
                currentActionSelectionState = ACTION_SELECTION_STATE_QUICK;
            }
        }
        private void buttonSlowSpellHandler() {
            if (currentActionSelectionState != ACTION_SELECTION_STATE_SLOW) {
                if (isMoving)
                    clearMovementRangeCells();
                else if (isSpelling)
                    clearSpellRangeCells();
                currentActionSelectionState = ACTION_SELECTION_STATE_SLOW;
            }
        }

        private void clearSpellRangeCells() {
            clearEffectRangeCells();
            if (spellRangeCells != null) {
                grid.SetCellColor(spellRangeCells, Color.white);
                spellRangeCells = null;
            }
            currentSelectedSpell = SELECTED_SPELL_NONE;
        }
        private void clearEffectRangeCells() {
            if (effectRangeCells != null) {
                grid.SetCellColor(effectRangeCells, Color.white);
                effectRangeCells = null;
            }
            currentTargetCell = null;
        }
        private void clearMovementRangeCells() {
            if (movementRangeCells != null) {
                grid.SetCellColor(movementRangeCells, Color.white);
                movementRangeCells = null;
            }
            clearMovementPathCells();
        }
        private void clearMovementPathCells() {
            if (movementPathCells != null) {
                grid.SetCellColor(movementPathCells, Color.white);
                movementPathCells = null;
            }
            currentTargetCell = null;
        }

        private void buttonSpellHandler(short spellIndex) {
            if (isSpelling) {
                // clear previous spell preview
                clearSpellRangeCells();

                // switch to selected spell
                currentSelectedSpell = spellIndex;

                // display spell range preview
                int priority = isQuickSpelling ? 0 : 1;

                int rangeType = localSpells[currentSelectedSpell].getRangeType(priority);
                int minRange = localSpells[currentSelectedSpell].getMinRange(priority);
                int maxRange = localSpells[currentSelectedSpell].getMaxRange(priority);

                spellRangeCells = grid.getCellsInRange(localEntity.getPriorityCurrentCell(), minRange, maxRange, rangeType);
                grid.SetCellColor(spellRangeCells, Color.blue);
            }
        }


        private void buttonConfirmHandler() {
            if (isAiming) {
                if (isSpelling) {
                    if (isAnySpellSelected) {
                        if (currentTargetCell != null) {
                            SpellInstance spellInstance = localSpells[currentSelectedSpell];
                            registerSpellAction(spellInstance.spell.id, currentTargetCell);
                            registerSpellGhost(spellInstance.spell.id, currentTargetCell);
                            buttonRootHandler();
                        }
                    }
                } else if (isMoving) {
                    if (movementPathCells != null) {
                        registerMovementAction(movementPathCells);
                        registerMovementGhost(movementPathCells);
                        buttonRootHandler();
                    }
                }
            }
        }
        private void buttonCancelHandler() {
            if (localActions.Count > 0) {
                localActions.Dequeue();
                localEntity.unstackGhost();
            }
        }
        private void buttonReadyHandler() {
            // send data to server, be ready
            // synchronizeActions();
        }

        #endregion

        public void targetAction(Cell target) {
            //if (isAiming) {
            if (isSpelling) {
                if (isAnySpellSelected) {
                    if (spellRangeCells != null && spellRangeCells.Contains(target)) {
                        // clear previous effect preview
                        clearEffectRangeCells();

                        // switch to targeted cell
                        currentTargetCell = target;

                        // display spell effect preview
                        int priority = isQuickSpelling ? 0 : 1;
                        int[] areaType = selectedSpell.getAreaType(priority);
                        int[] minArea = selectedSpell.getMinArea(priority);
                        int[] maxArea = selectedSpell.getMaxArea(priority);

                        effectRangeCells = grid.getCellsInRanges(target, minArea, maxArea, areaType);
                        grid.SetCellColor(effectRangeCells, Color.red);
                    }

                }
            } else if (isMoving) {
                if (movementRangeCells != null && movementRangeCells.Contains(target)) {
                    // clear previous path preview
                    clearMovementPathCells();

                    // switch to selected path
                    currentTargetCell = target;

                    // display movement path preview
                    movementPathCells = new List<Cell>();
                    movementPathCells.Add(grid.GetCell(localEntity.currentCellId));
                    movementPathCells.Add(currentTargetCell);
                    grid.SetCellColor(movementPathCells, Color.yellow);
                    // register movement
                }
            }
        }

        private void registerSpellAction(string spellId, Cell target) {
            if (isQuickSpelling) {
                QuickSpellAction newAction = new QuickSpellAction();
                newAction.spellId = spellId;
                newAction.targetCellId = target.cellId;
                currentAction = newAction;
            } else if (isSlowSpelling) {
                SlowSpellAction newAction = new SlowSpellAction();
                newAction.spellId = spellId;
                newAction.targetCellId = target.cellId;
                currentAction = newAction;
            }
            currentAction.entityId = localEntity.entityId;
            //resolveAction(currentAction);
            // add to local actions
            localActions.Enqueue(currentAction);
        }
        private void registerMovementAction(List<Cell> path) {
            MovementAction newAction = new MovementAction();
            newAction.path = new int[path.Count];
            for (int i = 0; i < path.Count; i++)
                newAction.path[i] = path[i].cellId;
            currentAction = newAction;
            currentAction.entityId = localEntity.entityId;
            //resolveAction(currentAction);
            // add to local actions
            localActions.Enqueue(currentAction);
        }
        private void registerSpellGhost(string spellId, Cell target) {
            Cell finalPos = localEntity.currentCell;
            SpellData spell = DataManager.SPELL_DATA[spellId];
            if (isQuickSpelling) {
                foreach (EffectSpell effect in spell.effects) {
                    if (effect.quickHandler is EffectHandlerDash || effect.quickHandler is EffectHandlerWarp)
                        finalPos = target;
                }
            } else if (isSlowSpelling) {
                foreach (EffectSpell effect in spell.effects) {
                    if (effect.slowHandler is EffectHandlerDash || effect.quickHandler is EffectHandlerWarp)
                        finalPos = target;
                }
            }
            // create ghost
            localEntity.stackGhost(finalPos);
        }
        private void registerMovementGhost(List<Cell> path) {
            Cell finalPos = path[path.Count - 1];
            // create ghost
            localEntity.stackGhost(finalPos);
        }

        // ################################################################


        public override void prepareNewTurn(long startTurnTimestamp) {
            base.prepareNewTurn(startTurnTimestamp);
            serverDataTimeoutDate = endTurnDate.AddSeconds(CLIENT_WAIT_SERVER_SYNC_SECONDS);
            game1v1.gameViewController.SetTurnCount(currentTurn);
        }

        public override void startTurn() {
            base.startTurn();
        }

        public override void endTurn() {
            if (isActing) {
                synchronizeActions();
                CoroutineMaster.startCoroutine(waitForServerData());
            }
        }

        protected IEnumerator waitForServerData() {
            yield return new WaitForSecondsRealtime(CLIENT_WAIT_SERVER_SYNC_SECONDS);
            if (isWaitingSync) {
                // something went wrong, no data received from server
                Debug.LogError("waitForServerData() - no data received from server");
            }
        }

        public override void resolveTurn() {
            if (isSyncDone) {
                CoroutineMaster.startCoroutine(doResolveTurn());
            }
        }


        private IEnumerator doResolveTurn() {
            currentTurnState = TURN_STATE_RESOLVING;
            // trigger buff onTurnStartHandler
            foreach (Entity e in entityList.Values) {
                solver.resolveEntityBuffs(e, "onTurnStartHandler");
            }
            yield return new WaitForSeconds(0.5f); // wait for buff animations
                                                   // resolve action
            resolveActions(actions0);
            yield return new WaitWhile(ResolvingActions);
            resolveActions(actions1);
            yield return new WaitWhile(ResolvingActions);
            resolveActions(actions2);
            yield return new WaitWhile(ResolvingActions);
            // trigger buff onTurnEndHandler
            foreach (Entity e in entityList.Values) {
                solver.resolveEntityBuffs(e, "onTurnEndHandler");
            }
            // clear last execution data
            localEntity.clearGhost();
            // notify server turn ended on client
            currentTurnState = TURN_STATE_RESOLVING_DONE;

            if (checkForGameEnd()) {
                game1v1.QuitGame();
            } else {
                NetworkMasterClient.singleton.ClientReadyToPlay();
            }
        }

        protected bool resolvingActions = false;
        public bool ResolvingActions() {
            return resolvingActions;
        }

        public IEnumerator resolveActions(List<Data.Action> actions) {
            resolvingActions = true;
            List<Data.Action> fast = new List<Data.Action>();
            List<Data.Action> move = new List<Data.Action>();
            List<Data.Action> slow = new List<Data.Action>();
            foreach (Data.Action action in actions) {
                try {
                    switch (action.getPriority()) {
                        case 0:
                            fast.Add(action);
                            break;
                        case 1:
                            move.Add(action);
                            break;
                        default:
                            slow.Add(action);
                            break;
                    }
                } catch (Exception e) {
                    Debug.LogError(e.GetType().ToString() + " was catch. Turn resolution is interupted.");
                }
                resolvePriority(fast);
                yield return new WaitWhile(ResolvingPriority);
                resolvePriority(move);
                yield return new WaitWhile(ResolvingPriority);
                resolvePriority(slow);
                yield return new WaitWhile(ResolvingPriority);
            }
            actions.Clear();
            resolvingActions = false;
        }

        protected bool resolvingPriority = false;
        public virtual bool ResolvingPriority() {
            return resolvingPriority;
        }

        public IEnumerator resolvePriority(List<Data.Action> priority) {
            resolvingPriority = true;
            float maxDuration = 0;
            foreach (Data.Action a in priority) {
                resolveAction(a);
                maxDuration = Mathf.Max(maxDuration, resolveAction(a));
            }
            yield return new WaitForSeconds(maxDuration + 0.5f);
            resolvingPriority = false;
        }

        public float resolveAction(Data.Action action) {
            Entity e;
            if (!entityList.TryGetValue(action.entityId, out e)) {
                Debug.LogError("resolveAction() - can't find entity with id " + action.entityId);
            }
            if (action is MovementAction) {
                if (!action.isActionSkiped()) {
                    MovementAction movementAction = (MovementAction)action;
                    solver.resolveMovement(e, movementAction.path);
                    return movementAction.path.Length * 0.2f;
                } // TODO else give MP
            } else if (action is SpellAction) {
                if (!action.isActionSkiped()) {
                    SpellAction spellAction = (SpellAction)action;
                    Cell target = grid.GetCell(spellAction.targetCellId);
                    SpellData spell = DataManager.SPELL_DATA[spellAction.spellId];
                    solver.resolveSpell(spell, e, target, spellAction is QuickSpellAction);
                    return 0.5f;
                } // TODO else give AP
            }
            return 0;
        }


        public override void receiveActions(string actions) {
            base.receiveActions(actions);
            currentTurnState = TURN_STATE_SYNC_DONE;
        }

        public override void synchronizeActions() {
            //if (!isWaitingSync && !isSendingSync) {
            currentTurnState = TURN_STATE_SYNC_SEND;
            NetworkMasterClient.singleton.SyncTurnActions(this);
            currentTurnState = TURN_STATE_SYNC_WAIT;
            Debug.Log("synchronizeActions() done");
            //}
        }

        public override string generateTurnActionsJSON() {
            string output = "";
            for (int i = 0; i < 3; i++) {
                output += "\"actions" + i + "\":" + "[";
                if (localActions.Count > 0) {
                    Data.Action action = localActions.Dequeue();
                    output += action.toJSON();
                }
                output += "]";
                if (i < 2) {
                    output += ",";
                }
            }
            Debug.Log(output);
            return "{" + output + "}";
        }
    }
}