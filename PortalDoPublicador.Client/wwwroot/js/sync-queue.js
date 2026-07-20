// wwwroot/js/sync-queue.js

async function syncQueue(db) {
    try {
        // 1. Pega os dados locais
        const comandosPendentes = await dbLerTodos(db, 'FilaComandos');

        let ultimaSync = "2000-01-01T00:00:00Z";
        const configReq = await dbLerItem(db, 'Configuracoes', 'ultimaSync');
        if (configReq) ultimaSync = configReq.valor;

        // 2. Monta o Envelope
        const payloadEnvio = {
            ultimaSincronizacao: ultimaSync,
            comandosPush: comandosPendentes
        };

        // 3. Dispara a chamada ÚNICA
        const respostaHTTP = await fetch('/api/sync', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payloadEnvio)
        });

        if (!respostaHTTP.ok) {
            if (respostaHTTP.status === 409) {
                const erroConflito = await respostaHTTP.json();
                console.warn("Conflito detectado no servidor", erroConflito);
                
                // Avisa as abas abertas sobre o conflito
                if (self.clients) {
                    const clients = await self.clients.matchAll();
                    clients.forEach(client => {
                        client.postMessage({
                            type: 'CONFLITO_RESOLVIDO_PELO_SERVIDOR',
                            detalhes: erroConflito
                        });
                    });
                }
                
                // Em um cenário real, removeria apenas o comando em conflito.
                // Aqui estamos limpando os que falharam para não travar a fila.
                await limparComandosProcessados(db, comandosPendentes.map(c => ({ comandoId: c.id })));
            }
            return;
        }

        // 4. Desempacota a resposta
        const respostaServidor = await respostaHTTP.json();

        // A. Limpa a fila do que deu certo no Push
        if (respostaServidor.relatorioPush.length > 0) {
            await limparComandosProcessados(db, respostaServidor.relatorioPush);
        }

        // B. Salva os deltas novos no IndexedDB para o Blazor processar
        const dadosPull = respostaServidor.dadosPull;
        const temAtualizacoes = Object.keys(dadosPull.entidadesAtualizadas || {}).length > 0;
        const temExclusoes = Object.keys(dadosPull.entidadesExcluidas || {}).length > 0;
        
        if (temAtualizacoes || temExclusoes) {
            await dbSalvarItem(db, 'FilaPull', { dados: dadosPull });
        }

        // C. Atualiza a data com o relógio oficial do servidor
        await dbSalvarItem(db, 'Configuracoes', {
            chave: 'ultimaSync',
            valor: respostaServidor.novaDataSincronizacao
        });

    } catch (erro) {
        console.error('Erro na sincronização bidirecional:', erro);
        throw erro;
    }
}