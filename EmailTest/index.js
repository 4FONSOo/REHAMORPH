const express = require('express');
const cors = require('cors');
const nodemailer = require('nodemailer');
require('dotenv').config();

const app = express();
const PORT = 3000;

app.use(cors());
app.use(express.json());

// Obtém as credenciais do .env ou usa valores padrão (fallback)
const mailtrapUser = process.env.MAILTRAP_USER || '4b9ee014ca615f';
const mailtrapPass = process.env.MAILTRAP_PASS || 'a3ebdfd21db39c';

// Configuração do transportador do nodemailer para Mailtrap
const transporter = nodemailer.createTransport({
    host: 'sandbox.smtp.mailtrap.io',
    port: 587,
    auth: {
        user: mailtrapUser,
        pass: mailtrapPass
    }
});

app.get('/', (req, res) => {
    res.send('🚀 Servidor está rodando!');
});

app.post('/send-confirmation', async (req, res) => {
    const { email, token } = req.body;
    
    if (!email || !token) {
        return res.status(400).json({ error: 'Faltam dados no corpo da requisição.' });
    }

    console.log(`📧 Tentando enviar e-mail para ${email} com o token ${token}`);

    try {
        let info = await transporter.sendMail({
            from: '"Meu Jogo" <no-reply@REHAMORPH.com>',
            to: email,
            subject: "Confirmação de Conta",
            text: `Olá, o teu token de confirmação é: token=${token}`,
            html: `<p>Olá, o teu token de confirmação é: token = <strong>${token}</strong></p><a href="${token}"></a>`
        });
        console.log(`✅ E-mail enviado: ${info.messageId}`);
        res.json({ message: 'E-mail enviado com sucesso!' });
    } catch (error) {
        console.error("Erro ao enviar e-mail: " + error.message);
        res.status(500).json({ error: "Erro ao enviar e-mail: " + error.message });
    }
});

app.listen(PORT, () => {
    console.log(`✅ Servidor rodando na porta ${PORT}`);
});
