#!/bin/zsh
# NexBus - Start both backend and frontend
export DOTNET_ROOT="/Users/preetamtonyj/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"

BACKEND_DIR="$(dirname "$0")/backend"
FRONTEND_DIR="$(dirname "$0")/frontend"

echo "🔴 Killing any processes on ports 5047 and 4200..."
lsof -ti :5047 | xargs kill -9 2>/dev/null
lsof -ti :4200 | xargs kill -9 2>/dev/null
sleep 1

echo "🚀 Starting backend on http://localhost:5047 ..."
cd "$BACKEND_DIR" && dotnet run &
BACKEND_PID=$!

sleep 4

echo "🌐 Starting frontend on http://localhost:4200 ..."
cd "$FRONTEND_DIR" && npx ng serve --port 4200 &
FRONTEND_PID=$!

echo ""
echo "✅ NexBus is running!"
echo "   Backend API: http://localhost:5047/swagger"
echo "   Frontend:    http://localhost:4200"
echo ""
echo "Press Ctrl+C to stop both servers."

trap "kill $BACKEND_PID $FRONTEND_PID 2>/dev/null; echo 'Stopped.'" EXIT
wait
