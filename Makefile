.PHONY: dev

dev:
	@echo "Démarrage de la base de données (Docker)..."
	docker compose up db -d --wait
	@echo "Démarrage du backend .NET..."
	@trap 'kill 0' SIGINT; \
	(cd src/Web && dotnet run) & \
	(echo "En attente du backend..." && \
	 until curl -sf http://localhost:5280/api/public/site-settings > /dev/null 2>&1; do sleep 1; done && \
	 echo "Backend prêt! Démarrage de Vite..." && \
	 cd src/Web/vue-app && npm run dev) & \
	wait
