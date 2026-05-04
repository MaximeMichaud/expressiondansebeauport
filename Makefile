COMPOSE_PROD := docker compose --env-file .env.prod -f docker-compose.prod.yml

.PHONY: dev prod-up prod-down prod-restart prod-logs prod-ps

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

prod-up:
	$(COMPOSE_PROD) up -d --build --remove-orphans

prod-down:
	$(COMPOSE_PROD) down

prod-restart:
	$(COMPOSE_PROD) restart

prod-logs:
	$(COMPOSE_PROD) logs -f --tail=200

prod-ps:
	$(COMPOSE_PROD) ps
