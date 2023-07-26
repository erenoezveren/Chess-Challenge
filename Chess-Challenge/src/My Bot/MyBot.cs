using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;


public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        int cutoff = 100;
        int depth = 1;


        IDictionary<Move, double> bestMoves = new Dictionary<Move, double>(cutoff);

        Move[] moves = board.GetLegalMoves();
        Move bestMove = moves[0];
        Move cutoffMove = moves[0];
        double bestScore = -2000000;
        double cutoffScore = -2000000;

        int colour;
        if (board.IsWhiteToMove)
        {
            colour = 1;
        }
        else
        {
            colour = -1;
        }


        foreach (Move move in moves)
        {
            board.MakeMove(move);
            double score = Evaluate(board)*colour;
            if (score > bestScore)
            {
                bestMove = move;
                bestScore = score;
            }
            if (score > cutoffScore)
            {
                if (bestMoves.Count < cutoff) 
                {
                    bestMoves.Add(move, score);
                    cutoffMove = bestMoves.OrderBy(v => v.Value).First().Key;
                }
                else
                {
                    bestMoves.Remove(cutoffMove);
                    bestMoves.Add(move, score);
                    cutoffMove = bestMoves.OrderBy(v => v.Value).First().Key;
                }

            }
            board.UndoMove(move);


        }
        
        if (depth > 0)
        {
            return DeepThink(board, colour, bestMoves, depth, 0, cutoff);
        }
        return bestMove;
    }

    private double EasyThink(Board board, int colour)
    {
        Move[] moves = board.GetLegalMoves();
        if (moves.Length == 0)
        {
            return 1;
        }
        Move bestMove = moves[0];
        double bestScore = -2000000;

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            double score = Evaluate(board);
            if (score * colour > bestScore)
            {
                bestMove = move;
                bestScore = score;
            }
        
            board.UndoMove(move);

        }
        return bestScore;
    }

    private Move DeepThink(Board board, int colour, IDictionary<Move, double> bestMoves, int depth, int currentDepth, int cutoff)
    {

        Console.WriteLine(currentDepth);

        foreach (var kvp in bestMoves)
        {
            Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
        }
        Move[] oldMoves = bestMoves.Keys.ToArray();
        bestMoves.Clear();

        Move cutoffMove = oldMoves[0];
        double cutoffScore = -2000000;


        foreach (Move oldMove in oldMoves)
        {
            board.MakeMove(oldMove);
            Move[] moves = board.GetLegalMoves();

            foreach (Move move in moves)
            {
                board.MakeMove(move);
                double score = Evaluate(board) * colour;
                if (score > cutoffScore)
                {
                    if ((bestMoves.Count < cutoff) && (!bestMoves.ContainsKey(move)))
                    {   
              
                        bestMoves.Add(move, score);
                        cutoffMove = bestMoves.OrderBy(v => v.Value).First().Key;
                    }
                    else if (!bestMoves.ContainsKey(move))
                    {
                        bestMoves.Remove(cutoffMove);
                        bestMoves.Add(move, score);
                        cutoffMove = bestMoves.OrderBy(v => v.Value).First().Key;

                    }

                }
                board.UndoMove(move);
            }

            board.UndoMove(oldMove);

        }
        if (depth == currentDepth)
        {
            return bestMoves.OrderBy(v => v.Value).Last().Key;
        }

        return DeepThink(board, colour, bestMoves, depth, currentDepth + 1, cutoff);

    }

    private double Evaluate(Board board) {
        double runningscore = 0;
        PieceList[] pieces = board.GetAllPieceLists();
        foreach (PieceList pl in pieces) {
            int direction;
            if(pl.IsWhitePieceList)
            {
                direction = 1;
            }
            else
            {
                direction = -1;
            }

            int value;
            switch (pl.TypeOfPieceInList)
            {
                case PieceType.Pawn:
                    value = 1;
                    break;
                case PieceType.Knight:
                    value = 3;
                    break;
                case PieceType.Bishop:
                    value = 3;
                    break;
                case PieceType.Rook:
                    value = 5;
                    break;
                case PieceType.Queen:
                    value = 9;
                    break;
                default:
                    value = 0;
                    break;

            }
            runningscore += value * direction * pl.Count;
            
        }
        if (board.IsInCheck())
        {
            runningscore += 0.5;
        }
        if (board.IsInCheckmate())
        {
            runningscore += 1000000;
        }

        return runningscore;
    }
}